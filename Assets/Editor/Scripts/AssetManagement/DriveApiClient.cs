using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using UnityEngine;
using UnityEngine.Networking;

namespace KillChord.Editor.AssetManagement
{
    /// <summary>
    ///     Google Drive API v3 への通信を Service Account のアクセストークン (Bearer認証) で行う。
    ///     前提: 対象フォルダ/ファイルが Service Account のメールアドレスに対して共有されていること。
    ///     Unity 6.3 (UnityWebRequestAsyncOperationのネイティブGetAwaiterに対応) を前提に async/await で実装。
    /// </summary>
    internal static class DriveApiClient
    {
        private const string FILES_ENDPOINT = "https://www.googleapis.com/drive/v3/files";
        private const string FOLDER_MIME_TYPE = "application/vnd.google-apps.folder";
        private const string UPLOAD_ENDPOINT =
            "https://www.googleapis.com/upload/drive/v3/files?uploadType=multipart&supportsAllDrives=true";
        private const int UPLOAD_TIMEOUT_SECONDS = 600;

        [Serializable]
        private class FileEntry
        {
            /// <summary> Google Drive API の files リソースから返されるファイル情報。 </summary>
            public string id;
            /// <summary> ファイルの表示名。 </summary>
            public string name;
            /// <summary> ファイルの MIME タイプ。 </summary>
            public string mimeType;
            /// <summary> ファイルの最終更新日時 (RFC 3339 形式)。 </summary>
            public string modifiedTime;
        }

        [Serializable]
        private class ListResponse
        {
            /// <summary> 取得されたファイル情報の配列。 </summary>
            public FileEntry[] files;
            /// <summary> 次ページへのトークン。ページネーション用。 </summary>
            public string nextPageToken;
        }

        /// <summary>
        ///     Google Drive 上のファイル/フォルダを表現するデータ型。
        /// </summary>
        public class DriveNode
        {
            /// <summary> ファイルの一意識別子。 </summary>
            public string Id;
            /// <summary> ファイルの表示名。 </summary>
            public string Name;
            /// <summary> ファイルの MIME タイプ。 </summary>
            public string MimeType;
            /// <summary> ファイルの最終更新日時。 </summary>
            public string ModifiedTime;
            /// <summary> フォルダかどうかを判定する。 </summary>
            public bool IsFolder => MimeType == FOLDER_MIME_TYPE;
        }

        /// <summary>
        ///     指定フォルダ内の全ファイル/フォルダをリストアップする。ページネーションに対応。
        /// </summary>
        /// <param name="folderId"> リストアップ対象の Google Drive フォルダ ID。 </param>
        /// <param name="credential"> Drive API への認証情報。 </param>
        /// <param name="ct"> 操作をキャンセルするためのトークン。 </param>
        /// <returns> フォルダ内のファイル/フォルダのリスト。 </returns>
        public static async Task<List<DriveNode>> ListChildrenAsync(string folderId, ServiceAccountCredential credential, CancellationToken ct = default)
        {
            var result = new List<DriveNode>();
            string pageToken = null;

            do
            {
                var q = Uri.EscapeDataString($"'{folderId}' in parents and trashed = false");
                var fields = Uri.EscapeDataString("files(id,name,mimeType,modifiedTime),nextPageToken");
                var url = $"{FILES_ENDPOINT}?q={q}&fields={fields}&pageSize=1000" +
                          $"&supportsAllDrives=true&includeItemsFromAllDrives=true";

                if (!string.IsNullOrEmpty(pageToken))
                {
                    url += $"&pageToken={Uri.EscapeDataString(pageToken)}";
                }

                var accessToken = await credential.GetAccessTokenForRequestAsync(cancellationToken: ct);
                using var request = UnityWebRequest.Get(url);
                request.timeout = 60;
                request.SetRequestHeader("Authorization", $"Bearer {accessToken}");
                var asyncOp = request.SendWebRequest();
                while (!asyncOp.isDone)
                {
                    ct.ThrowIfCancellationRequested();
                    await Task.Yield();
                }

                if (request.result != UnityWebRequest.Result.Success)
                {
                    throw new Exception(
                        $"Drive API list failed ({request.responseCode}): {request.error}\n{request.downloadHandler?.text}");
                }

                var parsed = JsonUtility.FromJson<ListResponse>(request.downloadHandler.text);
                if (parsed?.files != null)
                {
                    foreach (var f in parsed.files)
                    {
                        result.Add(new DriveNode
                        {
                            Id = f.id,
                            Name = f.name,
                            MimeType = f.mimeType,
                            ModifiedTime = f.modifiedTime
                        });
                    }
                }

                pageToken = parsed?.nextPageToken;
            } while (!string.IsNullOrEmpty(pageToken));

            return result;
        }

        /// <summary>
        ///     Google Drive からファイルをダウンロードして、ローカルパスに保存する。
        /// </summary>
        /// <param name="fileId"> ダウンロード対象の Google Drive ファイル ID。 </param>
        /// <param name="credential"> Drive API への認証情報。 </param>
        /// <param name="destinationAbsolutePath"> 保存先の絶対パス。ディレクトリが存在しない場合は自動作成する。 </param>
        /// <param name="ct"> 操作をキャンセルするためのトークン。 </param>
        public static async Task DownloadFileAsync(string fileId, ServiceAccountCredential credential, string destinationAbsolutePath, CancellationToken ct = default)
        {
            var dir = Path.GetDirectoryName(destinationAbsolutePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var accessToken = await credential.GetAccessTokenForRequestAsync(cancellationToken: ct);
            var url = $"{FILES_ENDPOINT}/{fileId}?alt=media&supportsAllDrives=true";
            using var request = UnityWebRequest.Get(url);
            request.SetRequestHeader("Authorization", $"Bearer {accessToken}");
            request.downloadHandler = new DownloadHandlerFile(destinationAbsolutePath)
            {
                removeFileOnAbort = true
            };

            var asyncOp = request.SendWebRequest();
            while (!asyncOp.isDone)
            {
                ct.ThrowIfCancellationRequested();
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                if (File.Exists(destinationAbsolutePath))
                {
                    File.Delete(destinationAbsolutePath);
                }
                throw new Exception($"Drive API download failed ({request.responseCode}): {request.error}");
            }
        }

        /// <summary>
        ///     ローカルファイルを Google Drive の指定フォルダへアップロードする。
        /// </summary>
        /// <remarks>
        ///     UnityWebRequest ではなく HttpClient を使用する。
        ///     UnityWebRequest はメインスレッドのループでしか進行しないため、
        ///     同期的に完了を待つ呼び出し元(パッケージ出力手順など)から使用できない。
        /// </remarks>
        /// <param name="sourceAbsolutePath"> アップロードするローカルファイルの絶対パス。 </param>
        /// <param name="folderId"> アップロード先の Google Drive フォルダ ID。 </param>
        /// <param name="credential"> Drive API への認証情報。書き込み可能なスコープが必要。 </param>
        /// <param name="ct"> 操作をキャンセルするためのトークン。 </param>
        /// <returns> 作成されたファイルの ID。 </returns>
        public static async Task<string> UploadFileAsync(
            string sourceAbsolutePath,
            string folderId,
            ServiceAccountCredential credential,
            CancellationToken ct = default)
        {
            if (!File.Exists(sourceAbsolutePath))
            {
                throw new FileNotFoundException($"アップロード対象のファイルが存在しません: {sourceAbsolutePath}");
            }

            var accessToken = await credential.GetAccessTokenForRequestAsync(cancellationToken: ct);

            // 1リクエストでメタデータと本体を送るため multipart/related を使用する。
            var metadata = "{\"name\":\"" + EscapeJsonString(Path.GetFileName(sourceAbsolutePath)) + "\"," +
                           "\"parents\":[\"" + EscapeJsonString(folderId) + "\"]}";

            using var content = new MultipartContent("related");
            var metadataContent = new StringContent(metadata, Encoding.UTF8);
            metadataContent.Headers.ContentType = new MediaTypeHeaderValue("application/json") { CharSet = "UTF-8" };
            content.Add(metadataContent);

            // ファイル全体をメモリへ載せず、ストリームのまま送信する。
            using var fileStream = File.OpenRead(sourceAbsolutePath);
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            content.Add(fileContent);

            using var http = new HttpClient();
            http.Timeout = TimeSpan.FromSeconds(UPLOAD_TIMEOUT_SECONDS);
            using var message = new HttpRequestMessage(HttpMethod.Post, UPLOAD_ENDPOINT) { Content = content };
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await http.SendAsync(message, ct);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Drive API upload failed ({(int)response.StatusCode}): {body}");
            }

            return JsonUtility.FromJson<FileEntry>(body)?.id;
        }

        /// <summary>
        ///     JSON の文字列リテラルとして安全な形へエスケープする。
        /// </summary>
        /// <param name="value"> エスケープ対象の文字列。 </param>
        /// <returns> エスケープ済みの文字列。 </returns>
        private static string EscapeJsonString(string value)
            => value.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}
