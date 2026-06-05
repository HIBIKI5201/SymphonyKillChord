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
        ///     Unity -batchMode -quit -executeMethod KillChord.Editor.AssetImporter.GoogleDriveBatchCI.StartBatchImport
        /// </summary>
        public static void StartBatchImport()
        {
            Debug.Log("[GoogleDriveBatchCI] Starting batch import from GitHub Actions...");

            // async void を避けるため、EditorApplication.delayCall でスケジュール登録
            EditorApplication.delayCall += StartBatchImportAsync;
        }

        private static async void StartBatchImportAsync()
        {
            try
            {
                // 進捗フラグをリセット
                SessionState.SetFloat(AssetImportSettings.PROGRESS_VALUE_KEY, 0f);
                SessionState.SetString(AssetImportSettings.PROGRESS_MESSAGE_KEY, "Initializing...");

                // 環境変数から API キーとフォルダIDを取得してダウンロード実行
                await GoogleDriveDownloader.DownloadWithEnvironmentVariablesAsync(onProgressUpdate: (progress, message) =>
                {
                    Debug.Log($"[GoogleDriveBatchCI] Progress: {message}");
                    SessionState.SetFloat(AssetImportSettings.PROGRESS_VALUE_KEY, progress);
                    SessionState.SetString(AssetImportSettings.PROGRESS_MESSAGE_KEY, message);
                });

                // ダウンロード完了後、インポート処理をキューに追加
                SessionState.SetFloat(AssetImportSettings.PROGRESS_VALUE_KEY, 1.0f);
                SessionState.SetString(AssetImportSettings.PROGRESS_MESSAGE_KEY, "Download complete. Waiting for import...");
                Debug.Log("[GoogleDriveBatchCI] Download complete. AutoBuilder will be triggered on import completion.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GoogleDriveBatchCI] Error during batch import: {ex.Message}");
                EditorApplication.Exit(1); // エラーで終了
            }
        }

        /// <summary>
        ///     インポート進捗を監視し、完了後に AutoBuilder を起動する。
        ///     SessionState の PROGRESS_VALUE_KEY が 1.0f 以上になったら、処理が完了したと判定する。
        /// </summary>
        private static void MonitorImportProgress()
        {
            // パフォーマンス最適化：定期的にチェック
            if (EditorApplication.timeSinceStartup - _lastProgressCheck < PROGRESS_CHECK_INTERVAL)
            {
                return;
            }

            _lastProgressCheck = EditorApplication.timeSinceStartup;

            // インポートキューが存在しないか、進捗が記録されていない場合はスキップ
            float progress = SessionState.GetFloat(AssetImportSettings.PROGRESS_VALUE_KEY, 0.0f);

            // 進捗が 1.0f 以上（完了状態）かつインポートキューがまだある場合、処理完了と判定
            if (progress >= 1.0f)
            {
                string queueJson = SessionState.GetString(AssetImportSettings.SESSION_KEY, "");
                if (string.IsNullOrEmpty(queueJson))
                {
                    // インポートキューが完全に空になったので、クリーンアップして AutoBuilder を起動
                    TriggerAutoBuilderOnComplete();
                }
            }
        }

        /// <summary>
        ///     インポート完了後に一時ファイルをクリーンアップし、AutoBuilder を起動する。
        /// </summary>
        private static void TriggerAutoBuilderOnComplete()
        {
            Debug.Log("[GoogleDriveBatchCI] All assets imported successfully. Triggering AutoBuilder...");

            // エディタ更新の待機を解除
            EditorApplication.update -= MonitorImportProgress;

            try
            {
                // 一時フォルダのクリーンアップ
                CleanupTemporaryFolder();

                // バッチモードで AutoBuilder を実行
                AutoBuilder.AutoBuilder.PerformMultipleBuilds(isBatchMode: true);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GoogleDriveBatchCI] Error during AutoBuilder trigger: {ex.Message}");
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




