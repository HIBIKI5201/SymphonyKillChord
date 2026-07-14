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
        ///     指定したビルドプロファイルによる自動ビルドを開始します。
        /// </summary>
        /// <param name="path"> ビルド出力先です。 </param>
        /// <param name="profiles"> 実行対象のビルドプロファイル一覧です。 </param>
        public static void Run(string path, params BuildProfile[] profiles)
        {
            if (profiles == null || profiles.Length == 0)
            {
                Debug.LogError(
                    "Build Profiles are not set.");

                return;
            }

            if (!TryGetActiveBuildProfileGuid(out string originalProfileGuid))
            {
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

                HasFailure = false
            };

            BuildSession.SaveSession(session);
            ShowBuildProgress(session, "自動ビルドを準備しています。");

            EditorApplication.delayCall += ResumeBuild;
        }

        /// <summary>
        ///     スクリプトリロード後に自動ビルドセッションを再開できるよう登録します。
        /// </summary>
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            EditorApplication.delayCall += Resume;
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

            BuildPlayerOptions options = CreateBuildPlayerOptions(profile, locationPath);

            // フォルダ生成。
            if (Directory.Exists(buildDir))
            {
                Directory.Delete(buildDir, true);
            }

            Directory.CreateDirectory(buildDir);

            AssetDatabase.Refresh(ImportAssetOptions.DontDownloadFromCacheServer);

            await WaitForEditorReady();

            // BuildはdelayCallから実行する。
            EditorApplication.delayCall += ExecutePlayerBuild;

            void ExecutePlayerBuild()
            {
                EditorApplication.delayCall -= ExecutePlayerBuild;

                bool hasFailure = false;
                try
                {
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

        /// <summary>
        ///     コンパイル・アセット更新・プレイヤービルドが終了するまで待機します。
        /// </summary>
        private static async ValueTask WaitForEditorReady()
        {
            await SymphonyTask.WaitUntil(() =>
            {
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

            // 念のためさらに1フレーム待つ。
            await Awaitable.NextFrameAsync();
        }

        public static string GetExtension(BuildTarget target)
        {
            return target switch
            {
                BuildTarget.StandaloneWindows => ".exe",
                BuildTarget.StandaloneWindows64 => ".exe",
                BuildTarget.Android => ".apk",
                BuildTarget.StandaloneOSX => ".app",
                _ => string.Empty
            };
        }

        /// <summary>
        /// BuildProfileの情報からBuildPlayerOptionsを生成する静的メソッド
        /// </summary>
        public static BuildPlayerOptions CreateBuildPlayerOptions(BuildProfile profile, string path = null)
        {
            BuildProfile.SetActiveBuildProfile(profile);
            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;

            // 有効なシーンの抽出
            string[] scenes = profile.GetScenesForBuild()
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                scenes = EditorBuildSettings.scenes
                    .Where(s => s.enabled)
                    .Select(s => s.path)
                    .ToArray();
            }

            return new BuildPlayerOptions()
            {
                scenes = scenes,
                target = target,
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
        /// </summary>
        /// <param name="session"> 終了する自動ビルドセッションです。 </param>
        private static void FinishBuildSession(BuildSession session)
        {
            BuildSession.ClearSession();
            EditorUtility.ClearProgressBar();

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

            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog("AutoBuilder", message, "OK");
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
