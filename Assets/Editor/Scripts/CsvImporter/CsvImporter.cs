using UnityEngine;
using UnityEditor;
using UnityGoogleDrive;
using System;
using System.Collections.Generic;
using UnityGoogleDrive.Data;
using System.IO;

namespace KillChord.Editor
{
    public static class CsvImporter
    {
        private const string FOLDER_ID = "1FwWBdOEPY-RoSGgqUxkR9pHYh-pMYOyU";
        private const string IMPORT_DIRECTORY = "Assets/StreamingAssets/ScenarioAuthoring";

        /// <summary>
        ///     CSVが置かれているGoogleDriveフォルダのIDでリスト取得を行う。
        /// </summary>
        [MenuItem("KillChord/Import CSV")]
        private static void ImportCsv()
        {
            Debug.Log("Importing CSV...");

            // アクセストークンが無い場合、GoogleDriveは401ではなく403を返すためライブラリの自動再認証が働かない。
            // そのため、リクエスト送信前に明示的に認証を行う。
            if (string.IsNullOrEmpty(AuthController.AccessToken))
            {
                AuthController.OnAccessTokenRefreshed += HandleAccessTokenRefreshed;
                AuthController.RefreshAccessToken();
                return;
            }

            SendListRequest();
        }

        /// <summary>
        ///     認証完了後にファイルリストの取得を開始する。
        /// </summary>
        private static void HandleAccessTokenRefreshed(bool success)
        {
            AuthController.OnAccessTokenRefreshed -= HandleAccessTokenRefreshed;

            if (!success)
            {
                Debug.LogError($"[{nameof(CsvImporter)}] GoogleDriveの認証に失敗しました。");
                return;
            }

            SendListRequest();
        }

        /// <summary>
        ///     指定フォルダ直下のファイルリストを取得するリクエストを送信する。
        /// </summary>
        private static void SendListRequest()
        {
            var request = GoogleDriveFiles.List();

            request.Fields = new List<string> { "files(id, name, mimeType)" };
            request.Q = $"'{FOLDER_ID}' in parents and (mimeType = 'text/csv' or mimeType = 'application/vnd.google-apps.spreadsheet')";
            request.Send().OnDone += list => HandleListReceived(request, list);
        }

        /// <summary>
        ///     取得したファイルリストを元に、各ファイルのダウンロード〜インポートを行う。
        /// </summary>
        private static void HandleListReceived(GoogleDriveFiles.ListRequest request, FileList list)
        {
            using (request)
            {
                // エラー時はレスポンス自体がnullになるため、参照前に必ず検証する。
                if (request.IsError)
                {
                    Debug.LogError($"[{nameof(CsvImporter)}] ファイルリストの取得に失敗しました。{request.Error}");
                    return;
                }

                if (list?.Files == null || list.Files.Count == 0)
                {
                    Debug.LogError($"[{nameof(CsvImporter)}] ファイルリストが空です。フォルダIDと共有設定を確認してください。");
                    return;
                }

                foreach (var file in list.Files)
                {
                    Debug.Log($"Found file: {file.Name} (ID: {file.Id}, MimeType: {file.MimeType})");
                }

                // 大量ファイルインポート時のリフレッシュ抑制。完了時に必ずStopすること。
                AssetDatabase.StartAssetEditing();

                int remaining = list.Files.Count;
                bool hasError = false;

                foreach (var file in list.Files)
                {
                    DownloadFile(file, success =>
                    {
                        if (!success) hasError = true;

                        remaining--;
                        if (remaining <= 0)
                        {
                            AssetDatabase.StopAssetEditing();
                            AssetDatabase.Refresh();

                            if (hasError)
                                Debug.LogWarning($"[{nameof(CsvImporter)}] 一部のファイルでエラーが発生しましたが、インポート処理は完了しました。");
                            else
                                Debug.Log($"[{nameof(CsvImporter)}] 全ファイルのインポートが完了しました。");
                        }
                    });
                }
            }
        }

        /// <summary>
        ///     1ファイルをダウンロード(またはExport)し、完了後にコールバックを呼ぶ。
        /// </summary>
        private static void DownloadFile(UnityGoogleDrive.Data.File file, Action<bool> onComplete)
        {
            var fileName = SanitizeFileName(file.Name) + ".csv";

            if (file.MimeType == "application/vnd.google-apps.spreadsheet")
            {
                // Googleスプレッドシート形式はExportでCSVとして取得する必要がある
                var exportRequest = GoogleDriveFiles.Export(file.Id, "text/csv");
                exportRequest.Send().OnDone += data =>
                {
                    using (exportRequest)
                    {
                        if (exportRequest.IsError || data?.Content == null)
                        {
                            Debug.LogError($"[{nameof(CsvImporter)}] Exportに失敗しました: {file.Name} ({exportRequest.Error})");
                            onComplete?.Invoke(false);
                            return;
                        }

                        SaveAndImport(fileName, data.Content);
                        onComplete?.Invoke(true);
                    }
                };
            }
            else
            {
                var downloadRequest = GoogleDriveFiles.Download(file.Id);
                downloadRequest.Send().OnDone += data =>
                {
                    using (downloadRequest)
                    {
                        if (downloadRequest.IsError || data?.Content == null)
                        {
                            Debug.LogError($"[{nameof(CsvImporter)}] ダウンロードに失敗しました: {file.Name} ({downloadRequest.Error})");
                            onComplete?.Invoke(false);
                            return;
                        }

                        SaveAndImport(fileName, data.Content);
                        onComplete?.Invoke(true);
                    }
                };
            }
        }

        /// <summary>
        ///     受け取ったバイト列をAssets配下に書き出し、Unityにインポートさせる。
        /// </summary>
        private static void SaveAndImport(string fileName, byte[] content)
        {
            if (!Directory.Exists(IMPORT_DIRECTORY))
            {
                Directory.CreateDirectory(IMPORT_DIRECTORY);
            }

            var assetPath = Path.Combine(IMPORT_DIRECTORY, fileName);
            System.IO.File.WriteAllBytes(assetPath, content);

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

            Debug.Log($"Imported: {assetPath}");
        }

        /// <summary>
        ///     GoogleDrive上のファイル名からOS上使用不可な文字を除去する。
        /// </summary>
        private static string SanitizeFileName(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }
            return Path.GetFileNameWithoutExtension(name);
        }
    }
}