using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace KillChord.Editor
{
    /// <summary>
    ///     GoogleDrive上の指定フォルダからCSV(スプレッドシート含む)を一括取得し、プロジェクトへ取り込むウィンドウ。
    ///     APIキーによる公開ファイルの読み取りのみを行うため、OAuth認証は不要。
    /// </summary>
    public sealed class CsvImporter : EditorWindow
    {
        [MenuItem("Tools/Import CSV")]
        private static void Open()
        {
            GetWindow<CsvImporter>("CSV Importer");
        }

        private void OnEnable()
        {
            _apiKey = EditorPrefs.GetString(API_KEY_PREFS_KEY, string.Empty);
            _folderId = EditorPrefs.GetString(FOLDER_ID_PREFS_KEY, DEFAULT_FOLDER_ID);
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "「リンクを知っている全員」で共有されたフォルダから、CSVとスプレッドシートを取得します。\n" +
                "APIキーはこのマシンのEditorPrefsにのみ保存され、リポジトリには含まれません。",
                MessageType.Info);

            EditorGUILayout.Space();

            using (var scope = new EditorGUI.ChangeCheckScope())
            {
                _apiKey = EditorGUILayout.PasswordField("API Key", _apiKey);
                _folderId = EditorGUILayout.TextField("Folder ID", _folderId);

                if (scope.changed)
                {
                    EditorPrefs.SetString(API_KEY_PREFS_KEY, _apiKey);
                    EditorPrefs.SetString(FOLDER_ID_PREFS_KEY, _folderId);
                }
            }

            EditorGUILayout.LabelField("保存先", IMPORT_DIRECTORY);

            EditorGUILayout.Space();

            var canExecute = !_isRunning
                && !string.IsNullOrWhiteSpace(_apiKey)
                && !string.IsNullOrWhiteSpace(_folderId);

            using (new EditorGUI.DisabledScope(!canExecute))
            {
                if (GUILayout.Button(_isRunning ? "実行中..." : "インポート", GUILayout.Height(BUTTON_HEIGHT)))
                {
                    ExecuteImport();
                }
            }

            if (string.IsNullOrEmpty(_resultMessage))
            {
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(_resultMessage, _isSucceeded ? MessageType.Info : MessageType.Error);
        }

        /// <summary>
        ///     フォルダ内のファイル一覧取得からプロジェクトへの保存までを実行する。
        /// </summary>
        private async void ExecuteImport()
        {
            _isRunning = true;
            _isSucceeded = false;
            _resultMessage = string.Empty;

            try
            {
                var entries = await FetchFileEntriesAsync();
                if (entries == null)
                {
                    return;
                }

                // 全ファイルの取得に成功してから書き込むことで、一部だけ更新される状態を避ける。
                var downloads = await DownloadAllAsync(entries);
                if (downloads == null)
                {
                    return;
                }

                if (!ConfirmOverwrite(downloads))
                {
                    _resultMessage = "キャンセルしました。";
                    return;
                }

                WriteAll(downloads);

                _isSucceeded = true;
                _resultMessage = $"{downloads.Count} 件のインポートが完了しました。";
                Debug.Log($"[{nameof(CsvImporter)}] {_resultMessage}");
            }
            catch (Exception exception)
            {
                _resultMessage = $"予期しないエラーが発生しました。\n{exception.Message}";
                Debug.LogError($"[{nameof(CsvImporter)}] {exception}");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                _isRunning = false;
                Repaint();
            }
        }

        /// <summary>
        ///     フォルダ直下の取り込み対象ファイルを、ページングを辿って全件取得する。
        ///     失敗時はnullを返す。
        /// </summary>
        private async Task<List<DriveFileEntry>> FetchFileEntriesAsync()
        {
            var entries = new List<DriveFileEntry>();
            var skippedCount = 0;
            var pageToken = string.Empty;

            do
            {
                EditorUtility.DisplayProgressBar(PROGRESS_TITLE, "ファイル一覧を取得中...", 0f);

                var query = UnityWebRequest.EscapeURL($"'{_folderId.Trim()}' in parents and trashed = false");
                var url = $"{DRIVE_API_URL}?q={query}&fields=nextPageToken,files(id,name,mimeType)" +
                          $"&pageSize={PAGE_SIZE}&key={_apiKey.Trim()}";

                if (!string.IsNullOrEmpty(pageToken))
                {
                    url += $"&pageToken={UnityWebRequest.EscapeURL(pageToken)}";
                }

                var response = await GetAsync(url);
                if (!response.IsSuccess)
                {
                    _resultMessage = $"ファイル一覧の取得に失敗しました。\n{response.Error}";
                    Debug.LogError($"[{nameof(CsvImporter)}] {_resultMessage}");
                    return null;
                }

                var list = JsonUtility.FromJson<DriveFileListResponse>(response.GetText());
                if (list?.files == null)
                {
                    _resultMessage = "ファイル一覧のレスポンスを解釈できませんでした。";
                    Debug.LogError($"[{nameof(CsvImporter)}] {_resultMessage}");
                    return null;
                }

                foreach (var entry in list.files)
                {
                    if (entry.mimeType == MIME_TYPE_SPREADSHEET || entry.mimeType == MIME_TYPE_CSV)
                    {
                        entries.Add(entry);
                    }
                    else
                    {
                        // ドキュメントなどCSV化できない形式は対象外とする。
                        skippedCount++;
                        Debug.Log($"[{nameof(CsvImporter)}] 対象外のためスキップ: {entry.name} ({entry.mimeType})");
                    }
                }

                pageToken = list.nextPageToken;
            }
            while (!string.IsNullOrEmpty(pageToken));

            if (entries.Count == 0)
            {
                _resultMessage = skippedCount > 0
                    ? $"取り込み対象のCSV・スプレッドシートがありませんでした。({skippedCount} 件は対象外)"
                    : "フォルダが空か、共有設定が「リンクを知っている全員」になっていません。";
                Debug.LogWarning($"[{nameof(CsvImporter)}] {_resultMessage}");
                return null;
            }

            return entries;
        }

        /// <summary>
        ///     全ファイルをメモリ上へ取得する。1件でも失敗した場合はnullを返す。
        /// </summary>
        private async Task<List<DownloadedCsv>> DownloadAllAsync(List<DriveFileEntry> entries)
        {
            var downloads = new List<DownloadedCsv>(entries.Count);
            var usedFileNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];

                EditorUtility.DisplayProgressBar(
                    PROGRESS_TITLE, $"取得中: {entry.name}", (float)i / entries.Count);

                var isSpreadsheet = entry.mimeType == MIME_TYPE_SPREADSHEET;
                var url = isSpreadsheet
                    ? $"{DRIVE_API_URL}/{entry.id}/export?mimeType={UnityWebRequest.EscapeURL(MIME_TYPE_CSV)}&key={_apiKey.Trim()}"
                    : $"{DRIVE_API_URL}/{entry.id}?alt=media&key={_apiKey.Trim()}";

                var response = await GetAsync(url);
                if (!response.IsSuccess)
                {
                    _resultMessage = $"取得に失敗したため中断しました: {entry.name}\n{response.Error}";
                    Debug.LogError($"[{nameof(CsvImporter)}] {_resultMessage}");
                    return null;
                }

                var fileName = BuildFileName(entry.name);
                if (!usedFileNames.Add(fileName))
                {
                    _resultMessage = $"保存名が重複するファイルがあるため中断しました: {fileName}\n" +
                                     "GoogleDrive側のファイル名を変更してください。";
                    Debug.LogError($"[{nameof(CsvImporter)}] {_resultMessage}");
                    return null;
                }

                downloads.Add(new DownloadedCsv(fileName, response.Data));
            }

            return downloads;
        }

        /// <summary>
        ///     既存ファイルを上書きしてよいかを確認する。上書き対象が無い場合は確認せずtrueを返す。
        /// </summary>
        private bool ConfirmOverwrite(List<DownloadedCsv> downloads)
        {
            var overwriteNames = new List<string>();

            foreach (var download in downloads)
            {
                if (File.Exists(ToAbsolutePath($"{IMPORT_DIRECTORY}/{download.FileName}")))
                {
                    overwriteNames.Add(download.FileName);
                }
            }

            if (overwriteNames.Count == 0)
            {
                return true;
            }

            return EditorUtility.DisplayDialog(
                "上書き確認",
                $"{overwriteNames.Count} 件の既存ファイルを上書きします。\n\n{string.Join("\n", overwriteNames)}",
                "上書きする",
                "キャンセル");
        }

        /// <summary>
        ///     取得済みのデータを一括で書き出す。
        /// </summary>
        private static void WriteAll(List<DownloadedCsv> downloads)
        {
            Directory.CreateDirectory(ToAbsolutePath(IMPORT_DIRECTORY));

            // 書き込みは同期処理のみで完結させ、StartAssetEditingが開きっぱなしにならないようにする。
            AssetDatabase.StartAssetEditing();

            try
            {
                foreach (var download in downloads)
                {
                    var assetPath = $"{IMPORT_DIRECTORY}/{download.FileName}";
                    File.WriteAllBytes(ToAbsolutePath(assetPath), download.Data);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.Refresh();
        }

        /// <summary>
        ///     指定URLへGETリクエストを送信し、完了を待つ。
        /// </summary>
        private static Task<HttpResponse> GetAsync(string url)
        {
            var completionSource = new TaskCompletionSource<HttpResponse>();
            var request = UnityWebRequest.Get(url);
            request.timeout = TIMEOUT_SECONDS;

            request.SendWebRequest().completed += _ =>
            {
                using (request)
                {
                    var isSuccess = request.result == UnityWebRequest.Result.Success;

                    // エラーメッセージにAPIキーを含むURLが混ざらないよう、本文とステータスのみを載せる。
                    var error = isSuccess
                        ? string.Empty
                        : $"HTTP {request.responseCode} / {request.error}\n{request.downloadHandler?.text}";

                    completionSource.SetResult(new HttpResponse(isSuccess, request.downloadHandler?.data, error));
                }
            };

            return completionSource.Task;
        }

        /// <summary>
        ///     GoogleDrive上のファイル名から、拡張子.csvを持つ安全なファイル名を組み立てる。
        /// </summary>
        private static string BuildFileName(string driveFileName)
        {
            var fileName = string.IsNullOrWhiteSpace(driveFileName) ? UNTITLED_FILE_NAME : driveFileName;

            foreach (var invalidChar in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(invalidChar, '_');
            }

            // 元の名前に拡張子が付いている場合のみ取り除き、それ以外のドットは名前として残す。
            if (fileName.EndsWith(CSV_EXTENSION, StringComparison.OrdinalIgnoreCase))
            {
                fileName = fileName.Substring(0, fileName.Length - CSV_EXTENSION.Length);
            }

            return fileName + CSV_EXTENSION;
        }

        /// <summary>
        ///     Assetsからの相対パスを絶対パスへ変換する。
        /// </summary>
        private static string ToAbsolutePath(string assetPath)
        {
            return $"{Path.GetDirectoryName(Application.dataPath)}/{assetPath}";
        }

        private const string DRIVE_API_URL = "https://www.googleapis.com/drive/v3/files";
        private const string MIME_TYPE_SPREADSHEET = "application/vnd.google-apps.spreadsheet";
        private const string MIME_TYPE_CSV = "text/csv";
        private const string IMPORT_DIRECTORY = "Assets/StreamingAssets/ScenarioAuthoring";
        private const string API_KEY_PREFS_KEY = "KillChord.CsvImporter.ApiKey";
        private const string FOLDER_ID_PREFS_KEY = "KillChord.CsvImporter.FolderId";
        private const string DEFAULT_FOLDER_ID = "1FwWBdOEPY-RoSGgqUxkR9pHYh-pMYOyU";
        private const string CSV_EXTENSION = ".csv";
        private const string UNTITLED_FILE_NAME = "Untitled";
        private const string PROGRESS_TITLE = "CSV Import";
        private const int TIMEOUT_SECONDS = 30;
        private const int PAGE_SIZE = 100;
        private const int BUTTON_HEIGHT = 28;

        private string _apiKey;
        private string _folderId;
        private string _resultMessage;
        private bool _isRunning;
        private bool _isSucceeded;

        /// <summary>
        ///     HTTPレスポンスの取得結果。
        /// </summary>
        private readonly struct HttpResponse
        {
            public HttpResponse(bool isSuccess, byte[] data, string error)
            {
                IsSuccess = isSuccess;
                Data = data;
                Error = error;
            }

            /// <summary> 取得に成功したか。 </summary>
            public bool IsSuccess { get; }
            /// <summary> レスポンス本文のバイト列。 </summary>
            public byte[] Data { get; }
            /// <summary> 失敗時のエラー内容。 </summary>
            public string Error { get; }

            /// <summary>
            ///     レスポンス本文を文字列として取得する。
            /// </summary>
            public string GetText()
            {
                return Data == null ? string.Empty : Encoding.UTF8.GetString(Data);
            }
        }

        /// <summary>
        ///     ダウンロード済みのCSV1件分。
        /// </summary>
        private readonly struct DownloadedCsv
        {
            public DownloadedCsv(string fileName, byte[] data)
            {
                FileName = fileName;
                Data = data;
            }

            /// <summary> 保存するファイル名。 </summary>
            public string FileName { get; }
            /// <summary> ファイルの中身。 </summary>
            public byte[] Data { get; }
        }

        /// <summary>
        ///     files.listのレスポンス。JsonUtilityの都合でAPIのフィールド名に合わせている。
        /// </summary>
        [Serializable]
        private sealed class DriveFileListResponse
        {
            public DriveFileEntry[] files;
            public string nextPageToken;
        }

        /// <summary>
        ///     files.listが返すファイル1件分。JsonUtilityの都合でAPIのフィールド名に合わせている。
        /// </summary>
        [Serializable]
        private sealed class DriveFileEntry
        {
            public string id;
            public string name;
            public string mimeType;
        }
    }
}
