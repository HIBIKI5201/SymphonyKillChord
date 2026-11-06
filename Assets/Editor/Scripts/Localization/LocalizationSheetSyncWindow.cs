using System;
using System.Collections.Generic;
using System.Linq;
using KillChord.Editor.Utility;
using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.Localization
{
    /// <summary>
    ///     訳語の正本（Google Drive のスプレッドシート）をローカライズのテーブルへ取り込むウィンドウです。
    ///     APIキーと正本フォルダのIDは、このマシンのEditorPrefsにだけ保存します。
    /// </summary>
    public sealed class LocalizationSheetSyncWindow : EditorWindow
    {
        /// <summary>
        ///     訳語の同期ウィンドウを開きます。
        /// </summary>
        [MenuItem(ToolConst.TOOLS_PATH + "Localization/訳語の正本と同期")]
        public static void Open()
        {
            GetWindow<LocalizationSheetSyncWindow>("訳語の同期");
        }

        private const int BUTTON_HEIGHT = 28;

        private string _apiKey;
        private string _folderId;
        private bool _isAutoSyncEnabled;
        private string _resultMessage;
        private bool _isRunning;
        private bool _isSucceeded;

        /// <summary>
        ///     EditorPrefs から設定を読み込みます。
        /// </summary>
        private void OnEnable()
        {
            _apiKey = EditorPrefs.GetString(LocalizationSheetSync.API_KEY_PREFS_KEY, string.Empty);
            _folderId = EditorPrefs.GetString(LocalizationSheetSync.FOLDER_ID_PREFS_KEY, string.Empty);
            _isAutoSyncEnabled = EditorPrefs.GetBool(LocalizationSheetSync.AUTO_SYNC_PREFS_KEY, true);
        }

        /// <summary>
        ///     設定欄・取り込みボタン・書き出しボタンと、前回の結果を描画します。
        /// </summary>
        private void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "訳語の正本は Google Drive の Localization フォルダにあるスプレッドシートです"
                + "（フォルダの場所は Notion の「重要リンク」を見てください）。\n"
                + "スプレッドシートの名前を String Table Collection 名（UICommon など）と同じにし、"
                + "先頭のシートに「キー / 日本語 / 英語 / 備考」の列を置きます。英語が空の行は、英語でも日本語を表示します。\n"
                + "マスターデータの訳文（MasterText）はキーを空にでき、日本語の文からキーを作ります。テーブルが無ければ作ります。\n"
                + "フォルダは「リンクを知っている全員」が閲覧できる共有にしてください。"
                + "APIキーはシナリオのCSV取り込みと共用で、このマシンのEditorPrefsにだけ保存します。",
                MessageType.Info);

            EditorGUILayout.Space();

            // 入力が変わったら EditorPrefs に保存する。
            using (var scope = new EditorGUI.ChangeCheckScope())
            {
                _apiKey = EditorGUILayout.PasswordField("API Key", _apiKey);
                _folderId = EditorGUILayout.TextField("正本フォルダID", _folderId);
                _isAutoSyncEnabled = EditorGUILayout.Toggle("起動時に自動で取り込む", _isAutoSyncEnabled);

                if (scope.changed)
                {
                    EditorPrefs.SetString(LocalizationSheetSync.API_KEY_PREFS_KEY, _apiKey.Trim());
                    EditorPrefs.SetString(LocalizationSheetSync.FOLDER_ID_PREFS_KEY, _folderId.Trim());
                    EditorPrefs.SetBool(LocalizationSheetSync.AUTO_SYNC_PREFS_KEY, _isAutoSyncEnabled);
                }
            }

            EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(_isRunning || !LocalizationSheetSync.IsConfigured))
            {
                if (GUILayout.Button(_isRunning ? "取り込み中..." : "正本から取り込む", GUILayout.Height(BUTTON_HEIGHT)))
                {
                    ExecuteSync();
                }
            }

            if (GUILayout.Button("テーブルをCSVに書き出す（正本の作り直し用）"))
            {
                ExecuteExport();
            }

            // 前回の実行結果があれば表示する。
            if (string.IsNullOrEmpty(_resultMessage))
            {
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(_resultMessage, _isSucceeded ? MessageType.Info : MessageType.Error);
        }

        /// <summary>
        ///     正本を取得し、差分を確認してからテーブルへ反映します。
        /// </summary>
        private async void ExecuteSync()
        {
            _isRunning = true;
            _resultMessage = string.Empty;

            try
            {
                (_isSucceeded, _resultMessage) = await LocalizationSheetSync.SyncAsync(ConfirmApply);
            }
            catch (Exception exception) when (!(exception is OperationCanceledException))
            {
                _isSucceeded = false;
                _resultMessage = $"予期しないエラーが発生しました。\n{exception.Message}";
                Debug.LogException(exception);
            }
            finally
            {
                _isRunning = false;
                Repaint();
            }
        }

        /// <summary>
        ///     書き出し先のフォルダを選ばせて、全テーブルをCSVに書き出します。
        /// </summary>
        private void ExecuteExport()
        {
            string directoryPath = EditorUtility.SaveFolderPanel("CSVの書き出し先", string.Empty, string.Empty);
            if (string.IsNullOrEmpty(directoryPath))
            {
                return;
            }

            int count = LocalizationSheetSync.ExportAll(directoryPath);
            _isSucceeded = true;
            _resultMessage = $"{count} 件のテーブルを書き出しました: {directoryPath}";
        }

        /// <summary>
        ///     差分の件数を見せて、反映してよいかを確認します。
        /// </summary>
        private static bool ConfirmApply(IReadOnlyList<LocalizationSheetSyncPlan> plans)
        {
            string summary = string.Join("\n", plans.Select(plan => plan.Summarize()));
            return EditorUtility.DisplayDialog(
                "訳語の取り込み",
                $"正本の内容をテーブルへ反映します。シートに無いキーはテーブルから消しません。\n\n{summary}",
                "反映する",
                "キャンセル");
        }
    }
}
