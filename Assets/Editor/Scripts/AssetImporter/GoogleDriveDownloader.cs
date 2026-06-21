using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using KillChord.Editor.AssetImporter.Settings;
using UnityEngine;
using UnityEngine.Networking;

namespace KillChord.Editor.AssetImporter
{
    /// <summary>
    ///     Google Drive APIを使用して、指定されたフォルダ内の最新のzipファイルをダウンロードし、その中から.unitypackageファイルを抽出して保存するためのクラス。
    /// </summary>
    public static class GoogleDriveDownloader
    {
        /// <summary>
        /// 環境変数からAPIキーとフォルダIDを取得し、ダウンロード・抽出を実行する。
        /// GitHub Secrets などのランナー環境での使用を想定している。
        /// APIキーは直接 Google Drive API の認証に使用されるため、リフレッシュトークンは不要。
        /// </summary>
        /// <param name="apiKeyEnvVar">APIキーが格納されている環境変数名（デフォルト: GOOGLE_DRIVE_API_KEY）</param>
        /// <param name="folderIdEnvVar">フォルダIDが格納されている環境変数名（デフォルト: GOOGLE_DRIVE_FOLDER_ID）</param>
        /// <param name="onProgressUpdate">進捗状況を更新するためのコールバック</param>
        public static async Task DownloadWithEnvironmentVariablesAsync(
            string apiKeyEnvVar = "GOOGLE_DRIVE_API_KEY",
            string folderIdEnvVar = "GOOGLE_DRIVE_FOLDER_ID",
            Action<float, string> onProgressUpdate = null)
        {
            try
            {
                // 環境変数から認証情報を取得
                string apiKey = Environment.GetEnvironmentVariable(apiKeyEnvVar);
                string folderId = Environment.GetEnvironmentVariable(folderIdEnvVar);

                if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(folderId))
                {
                    onProgressUpdate?.Invoke(0f, "環境変数が設定されていません。必要な環境変数: GOOGLE_DRIVE_API_KEY, GOOGLE_DRIVE_FOLDER_ID");
                    throw new Exception("Required environment variables are not set.");
                }

                // 一時的な保存先パスを設定（Library配下のアセット監視外のパス）
                string tempSavePath = Path.GetFullPath(AssetImportSettings.TEMP_EXTRACT_PATH);

                // ダウンロード・抽出を実行（APIキーを指定）
                await DownloadLatestZipAndExtractPackagesAsync(folderId, tempSavePath, apiKey, onProgressUpdate);

                Debug.Log("[GoogleDriveDownloader] Successfully downloaded and extracted packages from environment variables.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GoogleDriveDownloader] Error in DownloadWithEnvironmentVariablesAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// APIキーと各種認証情報を直接指定してダウンロード・抽出を実行する。
        /// ランナー環境で環境変数を標準的なキー名ではなく独自の仕様で渡す場合に使用。
        /// APIキーは直接認証に使用されるため、リフレッシュトークンは不要。
        /// </summary>
        /// <param name="apiKey">Google Drive APIキー</param>
        /// <param name="folderId">フォルダID</param>
        /// <param name="onProgressUpdate">進捗状況を更新するためのコールバック</param>
        public static async Task DownloadWithApiKeyAsync(
            string apiKey,
            string folderId,
            Action<float, string> onProgressUpdate = null)
        {
            try
            {
                if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(folderId))
                {
                    onProgressUpdate?.Invoke(0f, "必須パラメータが設定されていません。");
                    throw new Exception("Required parameters are empty.");
                }

                // 一時的な保存先パスを設定（Library配下のアセット監視外のパス）
                string tempSavePath = Path.GetFullPath(AssetImportSettings.TEMP_EXTRACT_PATH);

                // ダウンロード・抽出を実行（APIキーを直接使用）
                await DownloadLatestZipAndExtractPackagesAsync(folderId, tempSavePath, apiKey, onProgressUpdate);

                Debug.Log("[GoogleDriveDownloader] Successfully downloaded and extracted packages with provided API key.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GoogleDriveDownloader] Error in DownloadWithApiKeyAsync: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 最新のzipファイルをダウンロードし、内部の.unitypackageファイルをすべて抽出して保存する
        /// </summary>
        /// <param name="folderId">フォルダID</param>
        /// <param name="saveDirectoryPath">保存先ディレクトリパス</param>
        /// <param name="apiKey">Google Drive APIキー（null の場合はアクセストークンを使用）</param>
        /// <param name="onProgressUpdate">進捗状況を更新するためのコールバック</param>
        public static async Task DownloadLatestZipAndExtractPackagesAsync(string folderId,
            string saveDirectoryPath, string apiKey = null, Action<float, string> onProgressUpdate = null)
        {
            onProgressUpdate?.Invoke(0.1f, "Zipファイルを検索中...");

            folderId = folderId.Trim();

            // 1. 最新のzipファイルのIDを取得
            (string fileId, string foundFileName) = await GetLatestZipFileIdAsync(folderId, apiKey);
            if (string.IsNullOrEmpty(fileId))
            {
                onProgressUpdate?.Invoke(0f, "対象のzipファイルが見つかりませんでした。");
                return;
            }

            onProgressUpdate?.Invoke(0.3f, $"Zipダウンロード中 (Name: {foundFileName})...");

            // 2. zipファイルのバイナリデータを取得
            byte[] zipData = await DownloadZipBinaryAsync(fileId, apiKey);

            onProgressUpdate?.Invoke(0.7f, "Zip展開およびパッケージ抽出中...");

            // 3. 絶対パスの確定とディレクトリ作成
            string fullDirectoryPath = Path.GetFullPath(saveDirectoryPath);
            if (!Directory.Exists(fullDirectoryPath))
            {
                Directory.CreateDirectory(fullDirectoryPath);
            }

            // 4. メモリ上でzipを展開し、.unitypackageのみを抽出
            int extractedCount = 0;
            using (MemoryStream memoryStream = new MemoryStream(zipData))
            using (ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Read))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    // 拡張子が .unitypackage のファイルのみを対象とする
                    if (!entry.FullName.EndsWith(AssetImportSettings.EXT_UNITYPACKAGE, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    // zip内の階層構造を無視し、ファイル名のみを抽出して保存パスを生成
                    string fileName = Path.GetFileName(entry.FullName);
                    string destinationPath = Path.Combine(fullDirectoryPath, fileName);
                    entry.ExtractToFile(destinationPath, overwrite: true);
                    extractedCount++;
                }
            }

            onProgressUpdate?.Invoke(0.9f, $"抽出が完了: {extractedCount} 個のパッケージを抽出しました。インポートを開始します。");
        }

        /// <summary>
        ///     Google Drive APIの検索クエリを生成する。
        ///     指定されたフォルダIDがある場合は、そのフォルダ内で名前に「.zip」を含む最新ファイルを検索するクエリを生成する。
        /// </summary>
        /// <param name="folderId">フォルダID</param>
        /// <param name="apiKey">Google Drive APIキー（null の場合は認証が必要）</param>
        /// <returns>生成された検索URL</returns>
        private static string GenerateSearchURL(string folderId, string apiKey = null)
        {
            string query = "name contains '.zip' and trashed = false";
            if (!string.IsNullOrEmpty(folderId))
            {
                query += $" and '{folderId}' in parents";
            }

            string escapedQuery = UnityWebRequest.EscapeURL(query);
            string escapedOrderBy = UnityWebRequest.EscapeURL("modifiedTime desc");
            string escapedFields = UnityWebRequest.EscapeURL("files(id,name)");

            string url = $"{AssetImportSettings.DRIVE_API_FILES_ENDPOINT}?q={escapedQuery}&orderBy={escapedOrderBy}&pageSize=1&fields={escapedFields}";
            
            // APIキーを使用する場合、クエリパラメータとして追加
            if (!string.IsNullOrEmpty(apiKey))
            {
                url += $"&key={apiKey}";
            }
            
            return url;
        }

        /// <summary>
        /// Google Drive APIを使用して、指定されたフォルダ内の最新のzipファイルのIDと名前を取得する
        /// </summary>
        /// <param name="folderId">フォルダID</param>
        /// <param name="apiKey">Google Drive APIキー（null の場合はアクセストークンを使用）</param>
        /// <returns>ファイルIDと名前のタプル</returns>
        /// <exception cref="Exception">API呼び出しに失敗した場合</exception>
        public static async Task<(string id, string name)> GetLatestZipFileIdAsync(string folderId, string apiKey = null)
        {
            // APIキーを使用していない場合のみ、アクセストークンのリフレッシュを実行
            if (string.IsNullOrEmpty(apiKey))
            {
                await GoogleDriveAuthManager.RefreshAccessTokenAsync();
            }

            var settings = AssetImportSettings.instance;
            string url = GenerateSearchURL(folderId, apiKey);

            using UnityWebRequest request = UnityWebRequest.Get(url);
            
            // APIキーを使用していない場合のみ、Bearer トークン認証を設定
            if (string.IsNullOrEmpty(apiKey))
            {
                request.SetRequestHeader("Authorization", $"Bearer {settings.accessToken}");
            }
            
            var operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                // Google Drive APIが返却した詳細なJSONエラーメッセージを取得
                string errorDetails = (request.downloadHandler != null) ? request.downloadHandler.text : "詳細なし";
                throw new Exception($"HTTP Error: {request.error}\n【Google API 詳細レスポンス】\n{errorDetails}");
            }

            DriveFileListResponse response =
                JsonUtility.FromJson<DriveFileListResponse>(request.downloadHandler.text);

            if (response.files is { Length: > 0 })
            {
                return (response.files[0].id, response.files[0].name);
            }

            return (string.Empty, string.Empty);
        }

        /// <summary>
        /// Google Drive APIを使用して、指定されたファイルIDのzipファイルをダウンロードし、そのバイナリデータを返す
        /// </summary>
        /// <param name="fileId">ファイルID</param>
        /// <param name="apiKey">Google Drive APIキー（null の場合はアクセストークンを使用）</param>
        /// <returns>zipファイルのバイナリデータ</returns>
        /// <exception cref="Exception">API呼び出しに失敗した場合</exception>
        private static async Task<byte[]> DownloadZipBinaryAsync(string fileId, string apiKey = null)
        {
            var settings = AssetImportSettings.instance;
            string url = string.Format(AssetImportSettings.DRIVE_DOWNLOAD_ENDPOINT_TEMPLATE, fileId);

            // APIキーを使用する場合、クエリパラメータとして追加
            if (!string.IsNullOrEmpty(apiKey))
            {
                url += $"&key={apiKey}";
            }

            using UnityWebRequest request = UnityWebRequest.Get(url);
            
            // APIキーを使用していない場合のみ、Bearer トークン認証を設定
            if (string.IsNullOrEmpty(apiKey))
            {
                request.SetRequestHeader("Authorization", $"Bearer {settings.accessToken}");
            }
            
            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success)
            {
                // Google Drive APIが返却した詳細なJSONエラーメッセージを取得
                string errorDetails = (request.downloadHandler != null) ? request.downloadHandler.text : "詳細なし";
                throw new Exception($"HTTP Error: {request.error}\n【Google API 詳細レスポンス】\n{errorDetails}");
            }

            return request.downloadHandler.data;
        }

        /// <summary>
        /// Google Drive APIのファイルリストレスポンスを表すクラス。APIからのJSONレスポンスをデシリアライズするために使用される。
        /// </summary>
        [Serializable]
        private class DriveFileListResponse
        {
            public DriveFile[] files;
        }

        /// <summary>
        /// Google Drive APIのファイル情報を表すクラス。APIからのJSONレスポンスをデシリアライズするために使用される。
        /// </summary>
        [Serializable]
        private class DriveFile
        {
            public string id;
            public string name;
        }
    }
}