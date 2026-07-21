using System;
using System.IO;
using System.Threading.Tasks;
using KillChord.Editor.AssetImporter.Settings;
using KillChord.Editor.Utility;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
namespace KillChord.Editor.AssetImporter
{
    /// <summary>
    ///     Google Driveから最新のアセットパッケージをダウンロードして展開するためのエディタウィンドウ。
    /// </summary>
    public class AssetImportWindow : EditorWindow
    {
        // アセットインポーターのダウンロードと展開の進捗管理
        private string statusMessage = "待機中";
        private float progressValue;
        private bool isDownloading;

        // 最新Zipのファイル名表示
        private string latestFileName = "未取得 (API Key と Folder ID を入力して更新してください)";
        private bool isFetchingFileInfo;

        [MenuItem(ToolConst.WINDOW_PATH + nameof(AssetImportWindow))]
        public static void ShowWindow()
        {
            GetWindow<AssetImportWindow>(nameof(AssetImportWindow));
        }

        private void OnEnable()
        {
            FetchLatestFileInfo();
            progressValue = 0f;
            statusMessage = "待機中";
            isDownloading = false;
        }

        private void OnGUI()
        {
            var settings = AssetImportSettings.instance;
            string clientId = settings.clientId;
            string folderId = settings.folderId;

            GUILayout.Label("Google Drive 連携設定", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextField("Client ID", clientId);
                EditorGUILayout.TextField("Folder ID", folderId);
            }

            var isSettingsIncomplete = string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(folderId);

            if (isSettingsIncomplete)
            {
                EditorGUILayout.HelpBox("API Key、Folder ID、Save Path のいずれかが未設定です。設定を入力してから最新情報の更新をしてください。",
                    MessageType.Warning);

                return;
            }
            
            var isProcessing = isDownloading ||
                               SessionState.GetFloat(AssetImportSettings.PROGRESS_VALUE_KEY, -1f) >= 0f ||
                               isFetchingFileInfo;

            using (new EditorGUI.DisabledScope(isProcessing))
            {
                if (GUILayout.Button("Edit Settings"))
                {
                    SettingsService.OpenProjectSettings(AssetImportSettings.SETTINGS_PATH);
                }

                GUILayout.Space(10);

                // 最新のZipファイル名を表示し、ダウンロードしていないファイルがある場合は警告を表示する
                GUILayout.Label("Google Drive 上の最新ステータス", EditorStyles.boldLabel);
                if (latestFileName == AssetImportSettings.instance.lastDownloadedVersion)
                {
                    EditorGUILayout.HelpBox($"アセットバージョンは最新です！最新のZipファイル: {latestFileName}", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox($"ダウンロードしていないアセットがあります。最新のZipファイル: {latestFileName}",
                        MessageType.Warning);
                }

                if (GUILayout.Button("最新情報の更新"))
                {
                    FetchLatestFileInfo();
                }

                GUILayout.Space(10);

                if (GUILayout.Button("最新パッケージのZipをダウンロード"))
                {
                    if (string.IsNullOrEmpty(settings.refreshToken))
                    {
                        statusMessage = "エラー: Project Settings で認証を行ってください。トークンが無効です。";
                        progressValue = 0f;
                        return;
                    }

                    _ = ExecuteDownloadAsync();
                }
            }


            var value = SessionState.GetFloat(AssetImportSettings.PROGRESS_VALUE_KEY, -1f);
            var msg = SessionState.GetString(AssetImportSettings.PROGRESS_MESSAGE_KEY, "");
            if (value >= 0f)
            {
                UpdateProgress(value, msg);

                if (value >= 1.0f)
                {
                    if (settings.deleteAfterImport)
                    {
                        DeleteLocalPackages(AssetImportSettings.TEMP_EXTRACT_PATH);
                    }

                    SessionState.EraseFloat(AssetImportSettings.PROGRESS_VALUE_KEY);
                    SessionState.EraseString(AssetImportSettings.PROGRESS_MESSAGE_KEY);
                }
            }

            EditorGUI.ProgressBar(
                new Rect(10, position.height - 20, position.width - 20, 20),
                progressValue,
                statusMessage
            );
        }

        /// <summary>
        /// ダウンロードと展開の進捗状況を更新するためのコールバック関数。
        /// </summary>
        /// <param name="progress">進捗率 (0.0 - 1.0)</param>
        /// <param name="message">現在のステータスメッセージ</param>
        private void UpdateProgress(float progress, string message)
        {
            progressValue = progress;
            statusMessage = message;
            Repaint();
        }

        /// <summary>
        /// Google Driveから最新のzipファイルの情報を取得し、UIに反映する処理を開始する。
        /// </summary>
        private void FetchLatestFileInfo()
        {
            var settings = AssetImportSettings.instance;
            if (isFetchingFileInfo || string.IsNullOrEmpty(settings.refreshToken)) return;
            _ = FetchLatestFileInfoAsync();
        }

        /// <summary>
        /// Google Driveから最新のzipファイルの情報を非同期で取得し、UIに反映する処理。
        /// 注意：このメソッドは最新ファイル情報を取得するのみで、lastDownloadedVersion は更新しません。
        /// バージョンの更新は ExecuteDownloadAsync のダウンロード完了時に行われます。
        /// </summary>
        private async Task FetchLatestFileInfoAsync()
        {
            var settings = AssetImportSettings.instance;
            isFetchingFileInfo = true;
            latestFileName = "取得中...";
            Repaint();

            try
            {
                (string fileId, string fileName) =
                    await GoogleDriveDownloader.GetLatestZipFileIdAsync(settings.folderId);


                latestFileName = fileName ?? "対象ファイルが見つかりません";
            }
            catch (Exception e)
            {
                latestFileName = $"エラー: {e.Message}";
            }
            finally
            {
                isFetchingFileInfo = false;
                Repaint();
            }
        }

        /// <summary>
        /// Google Driveから最新のzipファイルをダウンロードし、展開してパッケージを抽出する非同期処理。
        /// ダウンロード成功後に lastDownloadedVersion を更新し、UI の警告表示を正確に保つ。
        /// </summary>
        private async Task ExecuteDownloadAsync()
        {
            try
            {
                var settings = AssetImportSettings.instance;
                isDownloading = true;
                await GoogleDriveDownloader.DownloadLatestZipAndExtractPackagesAsync(settings.folderId,
                    AssetImportSettings.TEMP_EXTRACT_PATH,
                    onProgressUpdate: UpdateProgress
                );

                AssetDatabase.Refresh();

                // ダウンロード完了後、最新ファイル情報を取得してバージョンを記録
                try
                {
                    (string fileId, string fileName) =
                        await GoogleDriveDownloader.GetLatestZipFileIdAsync(settings.folderId);

                    // ファイルが実際に存在することを確認してからバージョンを更新
                    if (!string.IsNullOrEmpty(fileId) && !string.IsNullOrEmpty(fileName))
                    {
                        settings.lastDownloadedVersion = fileName;
                        settings.Save();
                        Debug.Log($"[AssetImportWindow] バージョン情報を更新しました: {fileName}");
                    }
                }
                catch (Exception versionCheckEx)
                {
                    Debug.LogWarning($"[AssetImportWindow] バージョン更新に失敗しましたが、ダウンロード処理は成功しました: {versionCheckEx.Message}");
                }

                AssetPackageImporter.ResetAndStartImportQueue(AssetImportSettings.TEMP_EXTRACT_PATH);
            }
            catch (Exception e)
            {
                UpdateProgress(0f, $"エラー: {e.Message}");
                Debug.LogException(e);
                Repaint();
            }
            finally
            {
                isDownloading = false;
            }
        }

        /// <summary>
        /// ローカルの保存ディレクトリにあるインポート済みのunitypackageファイルを削除する。
        /// </summary>
        /// <param name="saveDirectoryPath"></param>
        private static void DeleteLocalPackages(string saveDirectoryPath)
        {
            string fullPath = Path.GetFullPath(saveDirectoryPath);
            if (Directory.Exists(fullPath))
            {
                string[] files = Directory.GetFiles(fullPath, $"*{AssetImportSettings.EXT_UNITYPACKAGE}",
                    SearchOption.TopDirectoryOnly);
                foreach (string file in files)
                {
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                    }
                }

                AssetDatabase.Refresh();
            }
        }
    }
}
#endif