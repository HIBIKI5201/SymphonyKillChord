using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using DevelopProducts.AutoBuild.Settings;
using UnityEngine;
using UnityEngine.Networking;

namespace DevelopProducts.AutoBuild
{
    /// <summary>
    ///     Google Drive APIを使用して、指定されたフォルダ内の最新のzipファイルをダウンロードし、その中から.unitypackageファイルを抽出して保存するためのクラス。
    /// </summary>
    public static class GoogleDriveDownloader
    {
        /// <summary>
        /// 最新のzipファイルをダウンロードし、内部の.unitypackageファイルをすべて抽出して保存する
        /// </summary>
        /// <param name="folderId">フォルダID</param>
        /// <param name="saveDirectoryPath">保存先ディレクトリパス</param>
        /// <param name="onProgressUpdate">進捗状況を更新するためのコールバック</param>
        public static async Task DownloadLatestZipAndExtractPackagesAsync(string folderId,
            string saveDirectoryPath, Action<float, string> onProgressUpdate = null)
        {
            onProgressUpdate?.Invoke(0.1f, "Zipファイルを検索中...");

            folderId = folderId.Trim();

            // 1. 最新のzipファイルのIDを取得
            (string fileId, string foundFileName) = await GetLatestZipFileIdAsync(folderId);
            if (string.IsNullOrEmpty(fileId))
            {
                onProgressUpdate?.Invoke(0f, "対象のzipファイルが見つかりませんでした。");
                return;
            }

            onProgressUpdate?.Invoke(0.3f, $"Zipダウンロード中 (Name: {foundFileName})...");

            // 2. zipファイルのバイナリデータを取得
            byte[] zipData = await DownloadZipBinaryAsync(fileId);

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
        /// <param name="folderId"></param>
        /// <returns></returns>
        private static string GenerateSearchURL(string folderId)
        {
            string query = "name contains '.zip' and trashed = false";
            if (!string.IsNullOrEmpty(folderId))
            {
                query += $" and '{folderId}' in parents";
            }

            string escapedQuery = UnityWebRequest.EscapeURL(query);
            string escapedOrderBy = UnityWebRequest.EscapeURL("modifiedTime desc");
            string escapedFields = UnityWebRequest.EscapeURL("files(id,name)");

            return $"{AssetImportSettings.DRIVE_API_FILES_ENDPOINT}?q={escapedQuery}&orderBy={escapedOrderBy}&pageSize=1&fields={escapedFields}";
        }

        /// <summary>
        /// Google Drive APIを使用して、指定されたフォルダ内の最新のzipファイルのIDと名前を取得する
        /// </summary>
        /// <param name="folderId">フォルダID</param>
        /// <returns>ファイルIDと名前のタプル</returns>
        /// <exception cref="Exception">API呼び出しに失敗した場合</exception>
        public static async Task<(string id, string name)> GetLatestZipFileIdAsync(string folderId)
        {
            // アクセストークンのリフレッシュを先に行う
            await GoogleDriveAuthManager.RefreshAccessTokenAsync();

            var settings = AssetImportSettings.instance;
            string url = GenerateSearchURL(folderId);

            using UnityWebRequest request = UnityWebRequest.Get(url);
            request.SetRequestHeader("Authorization", $"Bearer {settings.accessToken}");
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
        /// <returns>zipファイルのバイナリデータ</returns>
        /// <exception cref="Exception">API呼び出しに失敗した場合</exception>
        private static async Task<byte[]> DownloadZipBinaryAsync(string fileId)
        {
            var settings = AssetImportSettings.instance;
            string url = string.Format(AssetImportSettings.DRIVE_DOWNLOAD_ENDPOINT_TEMPLATE, fileId);

            using UnityWebRequest request = UnityWebRequest.Get(url);
            request.SetRequestHeader("Authorization", $"Bearer {settings.accessToken}");
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