using SymphonyFrameWork.Utility;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace KillChord.Editor.AutoBuilder
{
    /// <summary>
    ///     エディタ上で複数のビルドプロファイルを順番にビルドします。
    /// </summary>
    public static class AutoBuildExecuter
    {
        /// <summary> 自動ビルドセッションが実行中の場合はtrueです。 </summary>
        public static bool IsRunning => BuildSession.LoadSession().Running;
        [Serializable]
        private struct BuildSession
        {
            /// <summary> ビルド出力先です。 </summary>
            public string OutputPath;

            /// <summary> 実行対象のビルドプロファイルGUID一覧です。 </summary>
            public string[] ProfileGuids;

            /// <summary> 自動ビルド開始前のビルドプロファイルGUIDです。 </summary>
            public string OriginalProfileGuid;

            /// <summary> 現在処理しているプロファイル位置です。 </summary>
            public int CurrentIndex;

            /// <summary> 自動ビルド中の場合はtrueです。 </summary>
            public bool Running;

            /// <summary> いずれかのビルドが失敗した場合はtrueです。 </summary>
            public bool HasFailure;
            
            /// <summary> バッチモードフラグをセッションに保持します。 </summary>
            public bool ForceBatchMode;

            /// <summary>
            ///     保存済みの自動ビルドセッションを読み込みます。
            /// </summary>
            /// <returns> 保存されているセッションです。 </returns>
            public static BuildSession LoadSession()
            {
                string json = SessionState.GetString(SESSION_KEY, string.Empty);

                if (string.IsNullOrEmpty(json))
                {
                    return new BuildSession();
                }

                return JsonUtility.FromJson<BuildSession>(json);
            }

            /// <summary>
            ///     自動ビルドセッションを保存します。
            /// </summary>
            /// <param name="session"> 保存するセッションです。 </param>
            public static void SaveSession(BuildSession session)
            {
                string json = JsonUtility.ToJson(session);

                SessionState.SetString(SESSION_KEY, json);
            }

            /// <summary>
            ///     保存済みの自動ビルドセッションを削除します。
            /// </summary>
            public static void ClearSession()
            {
                SessionState.EraseString(SESSION_KEY);
            }

            private const string SESSION_KEY = "AUTO_BUILD_SESSION";
        }

        /// <summary>
        ///     再出力対象として記録された1件分のログです。
        /// </summary>
        [Serializable]
        private struct CapturedLogEntry
        {
            /// <summary> ログ種別（<see cref="LogType"/>の文字列表現）です。 </summary>
            public string Type;

            /// <summary> ログメッセージです。 </summary>
            public string Message;

            /// <summary> スタックトレースです。 </summary>
            public string StackTrace;
        }

        /// <summary>
        ///     ドメインリロードでコンソールがクリアされた後に再出力するためのログ群です。
        ///     BuildSessionとは別キーで保持し、FinishBuildSessionがセッションを消去した後も
        ///     リロード後の再出力まで生き残るようにする。
        /// </summary>
        [Serializable]
        private struct PendingLogReplay
        {
            /// <summary> ビルドが失敗した場合はtrueです。 </summary>
            public bool HasFailure;

            /// <summary> 記録されたログ一覧です。 </summary>
            public CapturedLogEntry[] Entries;
        }

        /// <summary>
        ///     再出力用ログを保持しすぎないための上限件数です。
        /// </summary>
        private const int MAX_CAPTURED_LOG_COUNT = 50;

        /// <summary> 再出力用ログを保存するSessionStateキーです。 </summary>
        private const string PENDING_LOG_KEY = "AUTO_BUILD_PENDING_LOG";

        /// <summary>
        ///     指定したビルドプロファイルによる自動ビルドを開始します。
        /// </summary>
        /// <param name="path"> ビルド出力先です。 </param>
        /// <param name="profiles"> 実行対象のビルドプロファイル一覧です。 </param>
        /// <param name="isBatchMode"> true の場合、バッチモードでの実行と判定し、ビルド完了後にエディタを終了する。false の場合は手動実行扱い。 </param>
        public static void Run(string path, BuildProfile[] profiles, bool isBatchMode = false)
        {
            if (profiles == null || profiles.Length == 0)
            {
                Debug.LogError("Build Profiles are not set.");
                ExitIfBatchMode(isBatchMode, exitCode: 1);
                return;
            }

            if (!TryGetActiveBuildProfileGuid(out string originalProfileGuid))
            {
                ExitIfBatchMode(isBatchMode, exitCode: 1);
                return;
            }

            BuildSession session = new()
            {
                OutputPath = path,

                ProfileGuids = profiles
                    .Select(p =>
                    {
                        string assetPath = AssetDatabase.GetAssetPath(p);

                        return AssetDatabase.AssetPathToGUID(assetPath);
                    })
                    .ToArray(),

                OriginalProfileGuid = originalProfileGuid,

                CurrentIndex = 0,

                Running = true,

                HasFailure = false,
                ForceBatchMode = isBatchMode
            };

            BuildSession.SaveSession(session);
            ShowBuildProgress(session, "自動ビルドを準備しています。");

            EditorApplication.delayCall += ResumeBuild;
        }

        /// <summary>
        ///     スクリプトリロード後に自動ビルドセッションを再開できるよう登録します。
        ///     あわせて、ビルド中のエラーログを捕捉する購読と、直前のドメインリロードで
        ///     コンソールから消えたログの再出力を行う。
        /// </summary>
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            Application.logMessageReceived += HandleLogMessage;
            ReplayPendingLogIfAny();

            EditorApplication.delayCall += Resume;
        }

        /// <summary>
        ///     自動ビルドセッション実行中に発生したエラー/例外/アサートログを捕捉し、
        ///     ドメインリロードを跨いで再出力できるようSessionStateへ記録する。
        /// </summary>
        /// <param name="message"> ログメッセージです。 </param>
        /// <param name="stackTrace"> スタックトレースです。 </param>
        /// <param name="type"> ログ種別です。 </param>
        private static void HandleLogMessage(string message, string stackTrace, LogType type)
        {
            if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert)
            {
                return;
            }

            if (!BuildSession.LoadSession().Running)
            {
                return;
            }

            AppendCapturedLog(type, message, stackTrace);
        }

        /// <summary>
        ///     再出力用ログ1件をSessionStateへ追記する。
        /// </summary>
        /// <param name="type"> ログ種別です。 </param>
        /// <param name="message"> ログメッセージです。 </param>
        /// <param name="stackTrace"> スタックトレースです。 </param>
        private static void AppendCapturedLog(LogType type, string message, string stackTrace)
        {
            PendingLogReplay replay = LoadPendingLogReplay();
            CapturedLogEntry[] entries = replay.Entries ?? Array.Empty<CapturedLogEntry>();

            if (entries.Length >= MAX_CAPTURED_LOG_COUNT)
            {
                return;
            }

            Array.Resize(ref entries, entries.Length + 1);
            entries[^1] = new CapturedLogEntry
            {
                Type = type.ToString(),
                Message = message,
                StackTrace = stackTrace
            };
            replay.Entries = entries;

            SavePendingLogReplay(replay);
        }

        /// <summary>
        ///     自動ビルドの成否を、再出力用ログへ記録する。
        ///     ログが1件も記録されていない場合は何もしない（再出力の必要がないため）。
        /// </summary>
        /// <param name="hasFailure"> いずれかのビルドが失敗した場合はtrueです。 </param>
        private static void SetPendingLogFailure(bool hasFailure)
        {
            PendingLogReplay replay = LoadPendingLogReplay();
            if (replay.Entries == null || replay.Entries.Length == 0)
            {
                return;
            }

            replay.HasFailure = hasFailure;
            SavePendingLogReplay(replay);
        }

        /// <summary>
        ///     直前のドメインリロードで消えた自動ビルドのログが記録されていれば、コンソールへ再出力する。
        /// </summary>
        private static void ReplayPendingLogIfAny()
        {
            string json = SessionState.GetString(PENDING_LOG_KEY, string.Empty);
            if (string.IsNullOrEmpty(json))
            {
                return;
            }

            SessionState.EraseString(PENDING_LOG_KEY);

            PendingLogReplay replay = JsonUtility.FromJson<PendingLogReplay>(json);
            if (replay.Entries == null || replay.Entries.Length == 0)
            {
                return;
            }

            Debug.LogWarning(
                $"[{nameof(AutoBuildExecuter)}] ビルドプロファイル復元時のドメインリロードでコンソールがクリアされたため、" +
                $"直前の自動ビルドで記録されたログを再出力します。({replay.Entries.Length}件)");

            foreach (CapturedLogEntry entry in replay.Entries)
            {
                string formatted = string.IsNullOrEmpty(entry.StackTrace)
                    ? entry.Message
                    : $"{entry.Message}\n{entry.StackTrace}";

                Debug.LogError(formatted);
            }

            string resultMessage = replay.HasFailure
                ? "自動ビルドは失敗しました。上記の再出力ログを確認してください。"
                : "自動ビルド中に上記のエラー/警告ログが記録されていました。";
            Debug.LogError($"[{nameof(AutoBuildExecuter)}] {resultMessage}");
        }

        /// <summary>
        ///     再出力用ログをSessionStateから読み込む。
        /// </summary>
        /// <returns> 保存されている再出力用ログです。未保存の場合は空です。 </returns>
        private static PendingLogReplay LoadPendingLogReplay()
        {
            string json = SessionState.GetString(PENDING_LOG_KEY, string.Empty);
            return string.IsNullOrEmpty(json)
                ? new PendingLogReplay { Entries = Array.Empty<CapturedLogEntry>() }
                : JsonUtility.FromJson<PendingLogReplay>(json);
        }

        /// <summary>
        ///     再出力用ログをSessionStateへ保存する。
        /// </summary>
        /// <param name="replay"> 保存する再出力用ログです。 </param>
        private static void SavePendingLogReplay(PendingLogReplay replay)
        {
            SessionState.SetString(PENDING_LOG_KEY, JsonUtility.ToJson(replay));
        }

        /// <summary>
        ///     保存済みセッションがあれば自動ビルドを再開します。
        /// </summary>
        private static async void Resume()
        {
            EditorApplication.delayCall -= Resume;

            await Awaitable.NextFrameAsync();

            BuildSession session = BuildSession.LoadSession();

            if (!session.Running)
            {
                EditorUtility.ClearProgressBar();
                return;
            }

            ShowBuildProgress(session, "ビルド環境の更新を待機しています。");
            EditorApplication.delayCall += ResumeBuild;
        }

        /// <summary>
        ///     現在のセッション位置に応じて次のビルドを予約します。
        /// </summary>
        private static void ResumeBuild()
        {
            EditorApplication.delayCall -= ResumeBuild;

            BuildSession session = BuildSession.LoadSession();

            if (!session.Running)
            {
                return;
            }

            // 完了していたら終了。
            if (session.CurrentIndex >= session.ProfileGuids.Length)
            {
                FinishBuildSession(session);
                return;
            }

            ShowBuildProgress(session, "次のビルドプロファイルを準備しています。");
            EditorApplication.delayCall += ExecuteBuild;
        }

        /// <summary>
        ///     エディタの準備完了後に現在のビルドプロファイルを実行します。
        /// </summary>
        private static async void ExecuteBuild()
        {
            EditorApplication.delayCall -= ExecuteBuild;

            await WaitForEditorReady();

            BuildSession session = BuildSession.LoadSession();

            if (!session.Running)
            {
                return;
            }

            try
            {
                await ExecuteBuildAsync(session);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                session.HasFailure = true;
                FinishBuildSession(session);
            }
        }

        /// <summary>
        ///     現在のセッション位置にあるビルドプロファイルを実行します。
        /// </summary>
        /// <param name="session"> 実行中の自動ビルドセッションです。 </param>
        private static async ValueTask ExecuteBuildAsync(BuildSession session)
        {
            try
            {
                string guid = session.ProfileGuids[session.CurrentIndex];

                string assetPath = AssetDatabase.GUIDToAssetPath(guid);

                BuildProfile profile = AssetDatabase.LoadAssetAtPath<BuildProfile>(assetPath);

                if (profile == null)
                {
                    Debug.LogError($"[{nameof(AutoBuildExecuter)}] Profile Missing : {guid}");

                    NextSession(true);

                    return;
                }

                Debug.Log($"[{nameof(AutoBuildExecuter)}] Start Build : {profile.name}");
                ShowBuildProgress(session, $"{profile.name} をビルドしています。");

                // Profile切替。
                BuildProfile.SetActiveBuildProfile(profile);

                await WaitForEditorReady();

                string[] scenes = profile.GetScenesForBuild()
                    .Where(s => s.enabled)
                    .Select(s => s.path)
                    .ToArray();

                // プロファイルに指定がなければグローバルを使用。
                if (scenes.Length == 0)
                {
                    scenes = EditorBuildSettings.scenes
                        .Where(s => s.enabled)
                        .Select(s => s.path)
                        .ToArray();
                }

                if (scenes.Length == 0)
                {
                    Debug.LogWarning($"[{nameof(AutoBuildExecuter)}] No scenes : {profile.name}");

                    NextSession(true);

                    return;
                }

                BuildTarget target = EditorUserBuildSettings.activeBuildTarget;

                string buildDir = Path.Combine(session.OutputPath, profile.name);
                string fileName = Application.productName + GetExtension(target);
                string locationPath = Path.Combine(buildDir, fileName);

                BuildPlayerWithProfileOptions options = CreateBuildPlayerOptions(profile, locationPath);

                // フォルダ生成。
                if (Directory.Exists(buildDir))
                {
                    Directory.Delete(buildDir, true);
                }

                Directory.CreateDirectory(buildDir);

                AssetDatabase.Refresh(ImportAssetOptions.DontDownloadFromCacheServer);

                await WaitForEditorReady();

                int executePlayerBuildRetryCount = 0;
                const int MAX_EXECUTE_PLAYER_BUILD_RETRY = 300;
                
                // BuildはdelayCallから実行する。
                EditorApplication.delayCall += ExecutePlayerBuild;

                void ExecutePlayerBuild()
                {
                    EditorApplication.delayCall -= ExecutePlayerBuild;

                    if (EditorApplication.isCompiling || EditorApplication.isUpdating)
                    {
                        executePlayerBuildRetryCount++;

                        if (executePlayerBuildRetryCount > MAX_EXECUTE_PLAYER_BUILD_RETRY)
                        {
                            Debug.LogError(
                                $"[{nameof(AutoBuildExecuter)}] BuildPlayer retry limit exceeded : {profile.name}");

                            NextSession(true);
                            return;
                        }

                        EditorApplication.delayCall += ExecutePlayerBuild;
                        return;
                    }

                    bool hasFailure;
                    try
                    {
                        Debug.Log(
                            $"[{nameof(AutoBuildExecuter)}] About to call BuildPipeline.BuildPlayer for profile: {profile.name}");
                        
                        BuildReport report = BuildPipeline.BuildPlayer(options);
                        hasFailure = report.summary.result != BuildResult.Succeeded;

                        if (hasFailure)
                        {
                            Debug.LogError(
                                $"[{nameof(AutoBuildExecuter)}] Build Failed : {profile.name}");
                        }
                        else
                        {
                            Debug.Log(
                                $"[{nameof(AutoBuildExecuter)}] Build Succeeded : {profile.name}");
                        }
                    }
                    catch (Exception exception)
                    {
                        Debug.LogException(exception);
                        hasFailure = true;
                    }

                    NextSession(hasFailure);
                }

                void NextSession(bool hasFailure = false)
                {
                    session.CurrentIndex++;
                    session.HasFailure |= hasFailure;

                    BuildSession.SaveSession(session);
                    ShowBuildProgress(session, "次のビルドを準備しています。");

                    EditorApplication.delayCall += ResumeBuild;
                }
            }
            catch (TimeoutException timeoutException)
            {
                Debug.LogError(
                    $"[{nameof(AutoBuildExecuter)}] EditorReady timeout - Skipping profile: {session.ProfileGuids[session.CurrentIndex]}. {timeoutException.Message}");
                
                // タイムアウト時はプロファイルをスキップして次へ
                session.CurrentIndex++;
                session.HasFailure = true;
                
                BuildSession.SaveSession(session);
                ShowBuildProgress(session, "タイムアウトのため次のビルドを準備しています。");
                
                EditorApplication.delayCall += ResumeBuild;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                
                // 予期しない例外はプロファイルをスキップして次へ
                session.CurrentIndex++;
                session.HasFailure = true;
                
                BuildSession.SaveSession(session);
                ShowBuildProgress(session, "エラーのため次のビルドを準備しています。");
                
                EditorApplication.delayCall += ResumeBuild;
            }
        }

        /// <summary>
        ///     コンパイル・アセット更新・プレイヤービルドが終了するまで待機します。
        /// </summary>
        /// <param name="timeoutSeconds">タイムアウト時間（秒）。デフォルトは120秒</param>
        /// <exception cref="TimeoutException">指定時間内に準備ができなかった場合</exception>
        private static async ValueTask WaitForEditorReady(int timeoutSeconds = 120)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            await SymphonyTask.WaitUntil(() =>
            {
                if (stopwatch.Elapsed.TotalSeconds > timeoutSeconds)
                {
                    stopwatch.Stop();
                    throw new TimeoutException(
                        $"[{nameof(AutoBuildExecuter)}] EditorReady timeout exceeded {timeoutSeconds}s. " +
                        $"isCompiling={EditorApplication.isCompiling}, " +
                        $"isUpdating={EditorApplication.isUpdating}, " +
                        $"isBuildingPlayer={BuildPipeline.isBuildingPlayer}");
                }

                if (EditorApplication.isCompiling)
                {
                    return false;
                }

                if (EditorApplication.isUpdating)
                {
                    return false;
                }

                if (BuildPipeline.isBuildingPlayer)
                {
                    return false;
                }

                return true;
            });

            stopwatch.Stop();

            // 念のためさらに1フレーム待つ。
            await Awaitable.NextFrameAsync();
        }

        /// <summary>
        ///     ビルドターゲットと現在のビルド設定に対応する拡張子を取得します。
        /// </summary>
        /// <param name="target"> ビルドターゲットです。 </param>
        /// <returns> 出力ファイルの拡張子です。 </returns>
        public static string GetExtension(BuildTarget target)
        {
            return target switch
            {
                BuildTarget.StandaloneWindows => ".exe",
                BuildTarget.StandaloneWindows64 => ".exe",
                BuildTarget.Android when EditorUserBuildSettings.buildAppBundle => ".aab",
                BuildTarget.Android => ".apk",
                BuildTarget.StandaloneOSX => ".app",
                _ => string.Empty
            };
        }

        /// <summary>
        ///     指定したビルドプロファイルに対応するBuildPlayerWithProfileOptionsを作成します。
        /// </summary>
        public static BuildPlayerWithProfileOptions CreateBuildPlayerOptions(BuildProfile profile, string path = null)
        {
            BuildProfile.SetActiveBuildProfile(profile);

            return new BuildPlayerWithProfileOptions
            {
                buildProfile = profile,
                locationPathName = path,
            };
        }

        /// <summary>
        ///     現在のビルドプロファイルGUIDを取得します。
        /// </summary>
        /// <param name="profileGuid"> 現在のプロファイルGUIDです。プラットフォームプロファイルの場合は空です。 </param>
        /// <returns> 復元に必要な情報を取得できた場合はtrueです。 </returns>
        private static bool TryGetActiveBuildProfileGuid(out string profileGuid)
        {
            profileGuid = string.Empty;
            BuildProfile activeProfile = BuildProfile.GetActiveBuildProfile();
            if (activeProfile == null)
            {
                return true;
            }

            string assetPath = AssetDatabase.GetAssetPath(activeProfile);
            profileGuid = AssetDatabase.AssetPathToGUID(assetPath);
            if (!string.IsNullOrEmpty(profileGuid))
            {
                return true;
            }

            Debug.LogError($"[{nameof(AutoBuildExecuter)}] 開始時のビルドプロファイルGUIDを取得できませんでした。Profile: {activeProfile.name}");
            return false;
        }

        /// <summary>
        ///     自動ビルドセッションを終了し、開始前のビルドプロファイルへ戻します。
        ///     バッチモード時はビルド成否に対応した終了コードでエディタを終了します。
        /// </summary>
        /// <param name="session"> 終了する自動ビルドセッションです。 </param>
        private static void FinishBuildSession(BuildSession session)
        {
            BuildSession.ClearSession();
            EditorUtility.ClearProgressBar();

            bool isBatchMode = Application.isBatchMode || session.ForceBatchMode;

            // バッチ実行時はログ出力後すぐに終了する。
            if (isBatchMode)
            {
                bool batchSucceeded = !session.HasFailure;
                string batchMessage = batchSucceeded
                    ? "自動ビルドが完了しました。"
                    : "自動ビルドが終了しました。Consoleでエラーを確認してください。";

                if (batchSucceeded)
                {
                    Debug.Log($"[{nameof(AutoBuildExecuter)}] {batchMessage}");
                }
                else
                {
                    Debug.LogError($"[{nameof(AutoBuildExecuter)}] {batchMessage}");
                }

                ExitIfBatchMode(session.ForceBatchMode, batchSucceeded ? 0 : 1);
                return;
            }

            // ビルドプロファイル復元によるドメインリロードでコンソールが消える前に、
            // 成否だけは再出力用ログへ記録しておく。
            SetPendingLogFailure(session.HasFailure);

            bool restored = TryRestoreBuildProfile(session.OriginalProfileGuid);
            bool succeeded = !session.HasFailure && restored;
            string message = succeeded
                ? "自動ビルドが完了し、開始前のビルドプロファイルへ戻しました。"
                : "自動ビルドが終了しました。Consoleでエラーを確認してください。";

            if (succeeded)
            {
                Debug.Log($"[{nameof(AutoBuildExecuter)}] {message}");
            }
            else
            {
                Debug.LogError($"[{nameof(AutoBuildExecuter)}] {message}");
            }

            EditorUtility.DisplayDialog("AutoBuilder", message, "OK");
        }

        /// <summary>
        ///     バッチモード、または forceBatchMode が true の場合のみ、エディタプロセスを指定されたコードで終了する。
        ///     これにより、手動実行時のエディタ強制終了を回避できる。
        /// </summary>
        /// <param name="forceBatchMode"> true の場合、Application.isBatchMode に関わらずバッチモード判定を行う。 </param>
        /// <param name="exitCode"> 終了コード（0: 成功、1: 失敗）。 </param>
        public static void ExitIfBatchMode(bool forceBatchMode, int exitCode)
        {
            bool shouldExit = Application.isBatchMode || forceBatchMode;
            if (shouldExit)
            {
                Debug.Log($"[{nameof(AutoBuildExecuter)}] Exiting Unity editor with code {exitCode} (Batch Mode)");
                EditorApplication.Exit(exitCode);
            }
        }

        /// <summary>
        ///     指定GUIDのビルドプロファイルへ戻します。
        /// </summary>
        /// <param name="profileGuid"> 復元対象GUIDです。空の場合はプラットフォームプロファイルへ戻します。 </param>
        /// <returns> 復元できた場合はtrueです。 </returns>
        private static bool TryRestoreBuildProfile(string profileGuid)
        {
            BuildProfile originalProfile = null;
            if (!string.IsNullOrEmpty(profileGuid))
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(profileGuid);
                originalProfile = AssetDatabase.LoadAssetAtPath<BuildProfile>(assetPath);
                if (originalProfile == null)
                {
                    Debug.LogError($"[{nameof(AutoBuildExecuter)}] 開始時のビルドプロファイルが見つかりません。GUID: {profileGuid}");
                    return false;
                }
            }

            try
            {
                if (BuildProfile.GetActiveBuildProfile() != originalProfile)
                {
                    BuildProfile.SetActiveBuildProfile(originalProfile);
                }

                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError($"[{nameof(AutoBuildExecuter)}] 開始時のビルドプロファイルへ戻せませんでした。{exception}");
                return false;
            }
        }

        /// <summary>
        ///     自動ビルド中であることと現在の進捗をエディタへ表示します。
        /// </summary>
        /// <param name="session"> 表示対象の自動ビルドセッションです。 </param>
        /// <param name="status"> 現在の処理内容です。 </param>
        private static void ShowBuildProgress(BuildSession session, string status)
        {
            if (Application.isBatchMode)
            {
                return;
            }

            int profileCount = session.ProfileGuids?.Length ?? 0;
            int displayIndex = profileCount > 0
                ? Mathf.Clamp(session.CurrentIndex + 1, 1, profileCount)
                : 0;
            float progress = profileCount > 0
                ? Mathf.Clamp01((float)session.CurrentIndex / profileCount)
                : 0f;

            EditorUtility.DisplayProgressBar(
                "AutoBuilder",
                $"自動ビルドを実行中です。 ({displayIndex}/{profileCount})\n{status}",
                progress);
        }
    }
}