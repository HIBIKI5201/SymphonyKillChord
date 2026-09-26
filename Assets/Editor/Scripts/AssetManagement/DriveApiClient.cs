using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private const string RESUMABLE_UPLOAD_ENDPOINT =
            "https://www.googleapis.com/upload/drive/v3/files?uploadType=resumable&supportsAllDrives=true";

        /// <summary> 1チャンクのサイズ。Drive APIの要求により256KBの倍数にする。 </summary>
        private const int CHUNK_SIZE_BYTES = 32 * 1024 * 1024;
        private const int CHUNK_TIMEOUT_SECONDS = 300;
        private const int MAX_CHUNK_RETRY_COUNT = 5;
        private const int RETRY_DELAY_SECONDS = 2;

        /// <summary> 未完了を示す308 Resume Incomplete。HttpStatusCodeに定義が無いため定数で持つ。 </summary>
        private const int STATUS_RESUME_INCOMPLETE = 308;
        private const int STATUS_SERVER_ERROR_MIN = 500;

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
        ///     <para>
        ///         セッションを開始してから分割送信する resumable upload を使用する。
        ///         パッケージのZIPはGB級になり得るため、1リクエストで送る multipart では
        ///         タイムアウトや切断のたびに最初からやり直しになる。
        ///     </para>
        ///     <para>
        ///         UnityWebRequest ではなく HttpClient を使用する。
        ///         UnityWebRequest はメインスレッドのループでしか進行しないため、
        ///         同期的に完了を待つ呼び出し元(パッケージ出力手順など)から使用できない。
        ///     </para>
        /// </remarks>
        /// <param name="sourceAbsolutePath"> アップロードするローカルファイルの絶対パス。 </param>
        /// <param name="folderId"> アップロード先の Google Drive フォルダ ID。 </param>
        /// <param name="credential"> Drive API への認証情報。書き込み可能なスコープが必要。 </param>
        /// <param name="progress"> 送信済みバイト数と総バイト数を受け取る進捗通知。省略可。 </param>
        /// <param name="ct"> 操作をキャンセルするためのトークン。 </param>
        /// <returns> 作成されたファイルの ID。 </returns>
        public static async Task<string> UploadFileAsync(
            string sourceAbsolutePath,
            string folderId,
            ServiceAccountCredential credential,
            Action<long, long> progress = null,
            CancellationToken ct = default)
        {
            if (!File.Exists(sourceAbsolutePath))
            {
                throw new FileNotFoundException($"アップロード対象のファイルが存在しません: {sourceAbsolutePath}");
            }

            var accessToken = await credential.GetAccessTokenForRequestAsync(cancellationToken: ct);

            using var http = new HttpClient();
            // 全体ではなくチャンク1つ分に対する上限として扱う。巨大ファイルで全体に上限を設けると必ず落ちる。
            http.Timeout = TimeSpan.FromSeconds(CHUNK_TIMEOUT_SECONDS);

            using var fileStream = File.OpenRead(sourceAbsolutePath);
            long totalSize = fileStream.Length;

            var sessionUri = await OpenUploadSessionAsync(
                http, accessToken, Path.GetFileName(sourceAbsolutePath), folderId, totalSize, ct);

            return await SendChunksAsync(http, accessToken, sessionUri, fileStream, totalSize, progress, ct);
        }

        /// <summary>
        ///     resumable upload のセッションを開始し、送信先URIを取得する。
        /// </summary>
        /// <param name="http"> 送信に使用するクライアント。 </param>
        /// <param name="accessToken"> Drive API のアクセストークン。 </param>
        /// <param name="fileName"> Drive上でのファイル名。 </param>
        /// <param name="folderId"> アップロード先の Google Drive フォルダ ID。 </param>
        /// <param name="totalSize"> 送信するファイルの総バイト数。 </param>
        /// <param name="ct"> 操作をキャンセルするためのトークン。 </param>
        /// <returns> チャンクの送信先となるセッションURI。 </returns>
        private static async Task<string> OpenUploadSessionAsync(
            HttpClient http,
            string accessToken,
            string fileName,
            string folderId,
            long totalSize,
            CancellationToken ct)
        {
            var metadata = "{\"name\":\"" + EscapeJsonString(fileName) + "\"," +
                           "\"parents\":[\"" + EscapeJsonString(folderId) + "\"]}";

            using var message = new HttpRequestMessage(HttpMethod.Post, RESUMABLE_UPLOAD_ENDPOINT)
            {
                Content = new StringContent(metadata, Encoding.UTF8, "application/json")
            };
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // 開始時点で総サイズを伝えると、容量不足などをチャンク送信前に検出できる。
            message.Headers.TryAddWithoutValidation("X-Upload-Content-Type", "application/octet-stream");
            message.Headers.TryAddWithoutValidation("X-Upload-Content-Length", totalSize.ToString());

            using var response = await http.SendAsync(message, ct);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new Exception($"Drive API upload session failed ({(int)response.StatusCode}): {body}");
            }

            var sessionUri = response.Headers.Location?.ToString();
            if (string.IsNullOrEmpty(sessionUri))
            {
                throw new Exception("Drive API upload session failed: セッションURIを取得できませんでした。");
            }

            return sessionUri;
        }

        /// <summary>
        ///     ファイルをチャンクへ分割して送信し、完了までを見届ける。
        /// </summary>
        /// <remarks>
        ///     チャンクの送信に失敗した場合はサーバへ受信済みバイト数を問い合わせ、その続きから再送する。
        ///     再試行の上限に達した場合だけ例外を投げる。
        /// </remarks>
        /// <param name="http"> 送信に使用するクライアント。 </param>
        /// <param name="accessToken"> Drive API のアクセストークン。 </param>
        /// <param name="sessionUri"> チャンクの送信先となるセッションURI。 </param>
        /// <param name="fileStream"> 送信元のファイルストリーム。 </param>
        /// <param name="totalSize"> 送信するファイルの総バイト数。 </param>
        /// <param name="progress"> 送信済みバイト数と総バイト数を受け取る進捗通知。 </param>
        /// <param name="ct"> 操作をキャンセルするためのトークン。 </param>
        /// <returns> 作成されたファイルの ID。 </returns>
        private static async Task<string> SendChunksAsync(
            HttpClient http,
            string accessToken,
            string sessionUri,
            FileStream fileStream,
            long totalSize,
            Action<long, long> progress,
            CancellationToken ct)
        {
            var buffer = new byte[CHUNK_SIZE_BYTES];
            long offset = 0;
            int retryCount = 0;

            while (offset < totalSize)
            {
                ct.ThrowIfCancellationRequested();

                // 再開後もずれないよう、送信のたびにサーバが期待する位置へ読み取り位置を合わせる。
                fileStream.Seek(offset, SeekOrigin.Begin);
                int read = await ReadChunkAsync(fileStream, buffer, ct);
                if (read <= 0)
                {
                    throw new Exception($"Drive API upload failed: 想定より早くファイル終端へ達しました ({offset}/{totalSize})。");
                }

                HttpResponseMessage response = null;
                try
                {
                    response = await SendChunkAsync(http, accessToken, sessionUri, buffer, read, offset, totalSize, ct);

                    // 308は「このチャンクは受け取った。続きを送れ」を意味し、失敗ではない。
                    if ((int)response.StatusCode == STATUS_RESUME_INCOMPLETE)
                    {
                        offset += read;
                        retryCount = 0;
                        progress?.Invoke(offset, totalSize);
                        continue;
                    }

                    if (response.IsSuccessStatusCode)
                    {
                        progress?.Invoke(totalSize, totalSize);
                        var body = await response.Content.ReadAsStringAsync();
                        return JsonUtility.FromJson<FileEntry>(body)?.id;
                    }

                    var errorBody = await response.Content.ReadAsStringAsync();
                    // 4xxは再送しても同じ結果になるため、その場で失敗として扱う。
                    if ((int)response.StatusCode < STATUS_SERVER_ERROR_MIN)
                    {
                        throw new Exception($"Drive API upload failed ({(int)response.StatusCode}): {errorBody}");
                    }

                    if (++retryCount > MAX_CHUNK_RETRY_COUNT)
                    {
                        throw new Exception(
                            $"Drive API upload failed ({(int)response.StatusCode}): 再試行の上限に達しました。\n{errorBody}");
                    }
                }
                catch (Exception e) when (e is HttpRequestException or TaskCanceledException && !ct.IsCancellationRequested)
                {
                    // 通信断やチャンクのタイムアウトは、受信済み位置を問い合わせたうえで再開できる。
                    if (++retryCount > MAX_CHUNK_RETRY_COUNT)
                    {
                        throw new Exception($"Drive API upload failed: 再試行の上限に達しました。\n{e}");
                    }
                }
                finally
                {
                    response?.Dispose();
                }

                // 再試行前に、サーバが実際に受け取った位置まで送信位置を巻き戻す。
                await Task.Delay(TimeSpan.FromSeconds(RETRY_DELAY_SECONDS), ct);
                offset = await QueryUploadedSizeAsync(http, accessToken, sessionUri, totalSize, ct);
                progress?.Invoke(offset, totalSize);
            }

            throw new Exception("Drive API upload failed: 全チャンクを送信しましたが完了応答を受け取れませんでした。");
        }

        /// <summary>
        ///     チャンク1つ分をバッファへ読み込む。
        /// </summary>
        /// <remarks>
        ///     Readは要求したサイズ未満を返し得るため、バッファが埋まるまで繰り返す。
        /// </remarks>
        /// <param name="fileStream"> 送信元のファイルストリーム。 </param>
        /// <param name="buffer"> 読み込み先のバッファ。 </param>
        /// <param name="ct"> 操作をキャンセルするためのトークン。 </param>
        /// <returns> 実際に読み込んだバイト数。 </returns>
        private static async Task<int> ReadChunkAsync(FileStream fileStream, byte[] buffer, CancellationToken ct)
        {
            int read = 0;
            while (read < buffer.Length)
            {
                int current = await fileStream.ReadAsync(buffer, read, buffer.Length - read, ct);
                if (current == 0) { break; }

                read += current;
            }

            return read;
        }

        /// <summary>
        ///     チャンク1つ分をセッションURIへ送信する。
        /// </summary>
        /// <param name="http"> 送信に使用するクライアント。 </param>
        /// <param name="accessToken"> Drive API のアクセストークン。 </param>
        /// <param name="sessionUri"> チャンクの送信先となるセッションURI。 </param>
        /// <param name="buffer"> 送信するデータを保持するバッファ。 </param>
        /// <param name="length"> バッファのうち送信する長さ。 </param>
        /// <param name="offset"> このチャンクの先頭がファイル全体で何バイト目かを示す位置。 </param>
        /// <param name="totalSize"> 送信するファイルの総バイト数。 </param>
        /// <param name="ct"> 操作をキャンセルするためのトークン。 </param>
        /// <returns> サーバからの応答。呼び出し側で破棄する。 </returns>
        private static async Task<HttpResponseMessage> SendChunkAsync(
            HttpClient http,
            string accessToken,
            string sessionUri,
            byte[] buffer,
            int length,
            long offset,
            long totalSize,
            CancellationToken ct)
        {
            using var message = new HttpRequestMessage(HttpMethod.Put, sessionUri)
            {
                Content = new ByteArrayContent(buffer, 0, length)
            };
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            message.Content.Headers.ContentRange =
                new ContentRangeHeaderValue(offset, offset + length - 1, totalSize);

            return await http.SendAsync(message, ct);
        }

        /// <summary>
        ///     サーバが受信済みのバイト数を問い合わせる。
        /// </summary>
        /// <remarks>
        ///     Rangeヘッダが返らない場合は1バイトも受信されていないため、先頭から送り直す。
        /// </remarks>
        /// <param name="http"> 送信に使用するクライアント。 </param>
        /// <param name="accessToken"> Drive API のアクセストークン。 </param>
        /// <param name="sessionUri"> 問い合わせ先のセッションURI。 </param>
        /// <param name="totalSize"> 送信するファイルの総バイト数。 </param>
        /// <param name="ct"> 操作をキャンセルするためのトークン。 </param>
        /// <returns> 次に送信すべき位置。 </returns>
        private static async Task<long> QueryUploadedSizeAsync(
            HttpClient http,
            string accessToken,
            string sessionUri,
            long totalSize,
            CancellationToken ct)
        {
            using var message = new HttpRequestMessage(HttpMethod.Put, sessionUri)
            {
                Content = new ByteArrayContent(Array.Empty<byte>())
            };
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            // 「bytes */総サイズ」は本体を送らずに受信済み位置だけを尋ねる問い合わせを意味する。
            message.Content.Headers.TryAddWithoutValidation("Content-Range", $"bytes */{totalSize}");

            using var response = await http.SendAsync(message, ct);
            if ((int)response.StatusCode != STATUS_RESUME_INCOMPLETE) { return 0; }

            var range = response.Headers.TryGetValues("Range", out var values)
                ? values.FirstOrDefault()
                : null;

            // 「bytes=0-<受信済みの最終バイト>」形式のため、次の送信位置は最終バイト+1となる。
            if (string.IsNullOrEmpty(range)) { return 0; }

            var lastByte = range[(range.LastIndexOf('-') + 1)..];
            return long.TryParse(lastByte, out var parsed) ? parsed + 1 : 0;
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
