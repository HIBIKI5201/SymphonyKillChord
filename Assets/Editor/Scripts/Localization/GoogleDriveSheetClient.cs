using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace KillChord.Editor.Localization
{
    /// <summary>
    ///     Google Drive のフォルダにあるスプレッドシートを、APIキーで一覧・CSV取得します。
    ///     フォルダは「リンクを知っている全員」が閲覧できる共有設定にしておく必要があります。
    /// </summary>
    public sealed class GoogleDriveSheetClient
    {
        /// <summary>
        ///     クライアントを生成します。
        /// </summary>
        /// <param name="apiKey"> Google Drive API を有効にしたAPIキーです。 </param>
        public GoogleDriveSheetClient(string apiKey)
        {
            _apiKey = (apiKey ?? string.Empty).Trim();
        }

        /// <summary>
        ///     フォルダ直下のスプレッドシートを、ページングを辿って全件取得します。
        /// </summary>
        /// <param name="folderId"> Google Drive のフォルダIDです。 </param>
        /// <returns> スプレッドシートの一覧と、失敗したときの理由です。 </returns>
        public async Task<(List<SheetFile> Files, string Error)> ListSpreadsheetsAsync(string folderId)
        {
            var files = new List<SheetFile>();
            string pageToken = string.Empty;

            do
            {
                string query = UnityWebRequest.EscapeURL(
                    $"'{folderId.Trim()}' in parents and trashed = false and mimeType = '{MIME_TYPE_SPREADSHEET}'");
                string url = $"{DRIVE_API_URL}?q={query}&fields=nextPageToken,files(id,name)"
                    + $"&pageSize={PAGE_SIZE}&key={_apiKey}";
                if (!string.IsNullOrEmpty(pageToken))
                {
                    url += $"&pageToken={UnityWebRequest.EscapeURL(pageToken)}";
                }

                (bool isSuccess, string text, string error) = await GetTextAsync(url);
                if (!isSuccess)
                {
                    return (null, $"スプレッドシートの一覧を取得できませんでした。\n{error}");
                }

                FileListResponse list = JsonUtility.FromJson<FileListResponse>(text);
                if (list?.files == null)
                {
                    return (null, "スプレッドシートの一覧を読み取れませんでした。");
                }

                foreach (FileEntry entry in list.files)
                {
                    files.Add(new SheetFile(entry.id, entry.name));
                }

                pageToken = list.nextPageToken;
            }
            while (!string.IsNullOrEmpty(pageToken));

            return (files, string.Empty);
        }

        /// <summary>
        ///     スプレッドシートの先頭のシートをCSVで取得します。
        /// </summary>
        /// <param name="fileId"> スプレッドシートのファイルIDです。 </param>
        /// <returns> CSVの文字列と、失敗したときの理由です。 </returns>
        public async Task<(string Csv, string Error)> ExportCsvAsync(string fileId)
        {
            string url = $"{DRIVE_API_URL}/{fileId}/export"
                + $"?mimeType={UnityWebRequest.EscapeURL(MIME_TYPE_CSV)}&key={_apiKey}";
            (bool isSuccess, string text, string error) = await GetTextAsync(url);
            if (!isSuccess)
            {
                return (null, error);
            }

            return (text, string.Empty);
        }

        /// <summary>
        ///     Google Drive 上のスプレッドシート1件です。
        /// </summary>
        public readonly struct SheetFile
        {
            /// <summary>
            ///     スプレッドシート1件を生成します。
            /// </summary>
            public SheetFile(string id, string name)
            {
                Id = id;
                Name = name;
            }

            /// <summary> ファイルIDです。 </summary>
            public string Id { get; }
            /// <summary> ファイル名です。String Table Collection名と同じにします。 </summary>
            public string Name { get; }
        }

        private const string DRIVE_API_URL = "https://www.googleapis.com/drive/v3/files";
        private const string MIME_TYPE_SPREADSHEET = "application/vnd.google-apps.spreadsheet";
        private const string MIME_TYPE_CSV = "text/csv";
        private const int TIMEOUT_SECONDS = 30;
        private const int PAGE_SIZE = 100;

        private readonly string _apiKey;

        /// <summary>
        ///     指定URLへGETリクエストを送り、本文を文字列で受け取ります。
        ///     エラーメッセージにAPIキーを含むURLが混ざらないよう、ステータスと本文だけを返します。
        /// </summary>
        private static Task<(bool IsSuccess, string Text, string Error)> GetTextAsync(string url)
        {
            var completionSource = new TaskCompletionSource<(bool, string, string)>();
            UnityWebRequest request = UnityWebRequest.Get(url);
            request.timeout = TIMEOUT_SECONDS;

            request.SendWebRequest().completed += _ =>
            {
                using (request)
                {
                    bool isSuccess = request.result == UnityWebRequest.Result.Success;
                    byte[] data = request.downloadHandler?.data;
                    string text = data == null ? string.Empty : Encoding.UTF8.GetString(data);
                    string error = isSuccess ? string.Empty : $"HTTP {request.responseCode} / {request.error}\n{text}";
                    completionSource.SetResult((isSuccess, text, error));
                }
            };

            return completionSource.Task;
        }

        /// <summary>
        ///     files.list のレスポンスです。JsonUtility の都合でAPIのフィールド名に合わせています。
        /// </summary>
        [Serializable]
        private sealed class FileListResponse
        {
            public FileEntry[] files;
            public string nextPageToken;
        }

        /// <summary>
        ///     files.list が返すファイル1件分です。JsonUtility の都合でAPIのフィールド名に合わせています。
        /// </summary>
        [Serializable]
        private sealed class FileEntry
        {
            public string id;
            public string name;
        }
    }
}
