using System;
using System.IO;
using KillChord.Editor.AssetImporter.Settings;
using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.AssetImporter
{
    /// <summary>
    ///     CI/CD環境でのアセットインポート処理を管理するためのクラス。
    ///     ドメインリロード後も処理を継続するため、[InitializeOnLoad]を使用してSessionState を監視する。
    ///     パッケージインポートが完了したら（progress >= 1.0f）、AutoBuilder を直接起動する。
    ///     
    ///     【GitHub Actions からのエントリポイント】
    ///     Unity -batchMode -quit -executeMethod KillChord.Editor.AssetImporter.GoogleDriveBatchCI.StartBatchImportAsync
    ///     または
    ///     環境変数を設定して呼び出し:
    ///     GOOGLE_DRIVE_API_KEY=xxx GOOGLE_DRIVE_FOLDER_ID=yyy Unity -batchMode -quit -executeMethod ...
    /// </summary>
    [InitializeOnLoad]
    public static class GoogleDriveBatchCI
    {
        private static double _lastProgressCheck;
        private const double PROGRESS_CHECK_INTERVAL = 0.1; // 100ms ごとにチェック

        static GoogleDriveBatchCI()
        {
            EditorApplication.update -= MonitorImportProgress;
            EditorApplication.update += MonitorImportProgress;
        }

        /// <summary>
        ///     【GitHub Actions 用エントリポイント】
        ///     環境変数からAPIキーとフォルダIDを取得し、アセットインポートを開始する。
        ///     完了後は自動的に AutoBuilder が起動され、ビルドが開始される。
        ///     
        ///     環境変数:
        ///     - GOOGLE_DRIVE_API_KEY: Google Drive APIキー
        ///     - GOOGLE_DRIVE_FOLDER_ID: ダウンロード対象フォルダID
        ///     
        ///     例:
        ///     Unity -batchMode -executeMethod KillChord.Editor.AssetImporter.GoogleDriveBatchCI.StartBatchImport
        /// </summary>
        public static void StartBatchImport()
        {
            Debug.Log("[GoogleDriveBatchCI] Starting batch import from GitHub Actions...");

            StartBatchImportAsync();
        }

        private static async void StartBatchImportAsync()
        {
            try
            {
                // 進捗フラグをリセット
                SessionState.SetFloat(AssetImportSettings.PROGRESS_VALUE_KEY, 0f);
                SessionState.SetString(AssetImportSettings.PROGRESS_MESSAGE_KEY, "Initializing...");

                // 一時的な保存先パスを設定
                string tempSavePath = Path.GetFullPath(AssetImportSettings.TEMP_EXTRACT_PATH);

                // 環境変数から API キーとフォルダIDを取得してダウンロード実行
                await GoogleDriveDownloader.DownloadWithEnvironmentVariablesAsync(onProgressUpdate: (progress, message) =>
                {
                    Debug.Log($"[GoogleDriveBatchCI] Progress: {message}");
                    SessionState.SetFloat(AssetImportSettings.PROGRESS_VALUE_KEY, progress);
                    SessionState.SetString(AssetImportSettings.PROGRESS_MESSAGE_KEY, message);
                });

                // ダウンロード完了後、インポートキューを開始する
                // 重要：AssetPackageImporter.ResetAndStartImportQueue() を呼ぶまで、PROGRESS_VALUE_KEY は 1.0f にしない
                // インポートキューが完成するまで、MonitorImportProgress() は待機し続ける
                Debug.Log("[GoogleDriveBatchCI] Download complete. Starting asset import queue...");
                AssetPackageImporter.ResetAndStartImportQueue(tempSavePath);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GoogleDriveBatchCI] Error during batch import: {ex.Message}");
                EditorApplication.Exit(1); // エラーで終了
            }
        }

        /// <summary>
        ///     インポート進捗を監視し、すべてのアセットインポートが完了したら AutoBuilder を起動する。
        ///     SessionState の PROGRESS_VALUE_KEY が 1.0f 以上かつインポートキューが空の場合、
        ///     すべてのインポートが完了したと判定し、ビルド処理を開始する。
        /// </summary>
        private static void MonitorImportProgress()
        {
            // パフォーマンス最適化：定期的にチェック
            if (EditorApplication.timeSinceStartup - _lastProgressCheck < PROGRESS_CHECK_INTERVAL)
            {
                return;
            }

            _lastProgressCheck = EditorApplication.timeSinceStartup;

            // インポート進捗と キュー状態を取得
            float progress = SessionState.GetFloat(AssetImportSettings.PROGRESS_VALUE_KEY, 0.0f);
            string queueJson = SessionState.GetString(AssetImportSettings.SESSION_KEY, "");

            // 進捗が 1.0f 以上（完了状態）かつインポートキューが空の場合、すべてのインポートが完了
            // queueJson が IsNullOrEmpty = true のとき、インポートキューは完全に空の状態
            if (progress >= 1.0f && string.IsNullOrEmpty(queueJson))
            {
                OnImportComplete();
            }
        }

        /// <summary>
        ///     インポート完了後に一時ファイルをクリーンアップし、エディタを終了する。
        /// </summary>
        private static void OnImportComplete()
        {

            // エディタ更新の待機を解除
            EditorApplication.update -= MonitorImportProgress;

            try
            {
                // 一時フォルダのクリーンアップ
                CleanupTemporaryFolder();
                Debug.Log("[GoogleDriveBatchCI] Import completed successfully.");
                EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GoogleDriveBatchCI] Error while finalizing import: {ex.Message}");
                EditorApplication.Exit(1);
            }
        }

        /// <summary>
        ///     Google Drive からダウンロードした一時ファイルディレクトリをクリーンアップする。
        /// </summary>
        private static void CleanupTemporaryFolder()
        {
            try
            {
                string tempPath = Path.GetFullPath(AssetImportSettings.TEMP_EXTRACT_PATH);
                if (Directory.Exists(tempPath))
                {
                    Debug.Log($"[GoogleDriveBatchCI] Cleaning up temporary folder: {tempPath}");
                    Directory.Delete(tempPath, recursive: true);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[GoogleDriveBatchCI] Failed to clean temporary folder: {ex.Message}");
            }
        }
    }
}




