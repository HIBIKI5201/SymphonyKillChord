using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.AssetManagement
{
    internal sealed class DriveImportSyncWindow : EditorWindow
    {
        /// <summary> 進捗表示ウィンドウ。 </summary>
        private static DriveImportSyncWindow window;

        /// <summary> ログ入力数の上限。超過時は古い入力から削除される。 </summary>
        private const int MaxLogEntries = 300;
        /// <summary> ログエントリのリスト。 </summary>
        private readonly List<LogEntry> logs = new();

        /// <summary> スクロール位置。 </summary>
        private Vector2 scroll;

        /// <summary> 同期ステータス文字列 (待機中/同期中/完了)。 </summary>
        private string status = "待機中";
        /// <summary> 現在処理中のファイル名。 </summary>
        private string current = "";

        /// <summary> 現在処理済みファイル数。 </summary>
        private int currentCount;
        /// <summary> 総処理対象ファイル数。 </summary>
        private int totalCount;
        
        /// <summary> 同期実行中であるかを示すフラグ。 </summary>
        private bool isRunning;
        /// <summary> 次の OnGUI で自動スクロール下へ移動させるか。 </summary>
        private bool scrollToBottom;

        /// <summary>
        /// Drive Import Sync ウィンドウを開く。
        /// </summary>
        [MenuItem("Window/Drive Import/Sync")]
        public static void Open()
        {
            window = GetWindow<DriveImportSyncWindow>("Drive Import");
            window.minSize = new Vector2(600, 400);
        }

        /// <summary>
        /// 同期を開始し、ウィンドウを初期化する。
        /// </summary>
        public static void StartSync()
        {
            Open();

            window.status = "同期中";
            window.current = "";
            window.currentCount = 0;
            window.totalCount = 0;
            window.Repaint();
            window.isRunning = true;
        }

        /// <summary>
        /// 同期完了をマークしてウィンドウを更新する。
        /// </summary>
        public static void Finish()
        {
            if (window == null)
                return;

            window.status = "完了";
            window.Repaint();
            window.isRunning = false;
        }

        /// <summary>
        /// 進捗状況 (現在数/総数) と処理中ファイル名を更新する。
        /// </summary>
        /// <param name="current"> 現在処理済みファイル数。 </param>
        /// <param name="total"> 総処理対象ファイル数。 </param>
        /// <param name="file"> 現在処理中のファイル名。 </param>
        public static void SetProgress(int current, int total, string file)
        {
            if (window == null) return;
            EditorApplication.delayCall += () =>
            {
                if (window == null) return;
                window.currentCount = current;
                window.totalCount = total;
                window.current = file;
                window.Repaint();
            };
        }

        /// <summary>
        /// ウィンドウにログエントリを追加する。
        /// </summary>
        /// <param name="type"> ログタイプ (情報/警告/エラー)。 </param>
        /// <param name="message"> ログメッセージ。 </param>
        private static void AddLog(LogType type, string message)
        {
            if (window == null) return;
            EditorApplication.delayCall += () =>
            {
                if (window == null) return;
                window.logs.Add(new LogEntry(type, message));
                if (window.logs.Count > MaxLogEntries)
                {
                    window.logs.RemoveAt(0);
                }

                window.scrollToBottom = true;

                window.Repaint();
            };
        }

        /// <summary> 情報ログを追加する。 </summary>
        public static void Log(string message)    => AddLog(LogType.Info, message);
        /// <summary> 警告ログを追加する。 </summary>
        public static void Warning(string message) => AddLog(LogType.Warning, message);
        /// <summary> エラーログを追加する。 </summary>
        public static void Error(string message)   => AddLog(LogType.Error, message);

        private void OnGUI()
        {
            // 状態
            EditorGUILayout.LabelField("状態", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(status);

            EditorGUILayout.Space();

            // 進捗バー
            Rect rect = GUILayoutUtility.GetRect(1, 20);

            EditorGUI.ProgressBar(
                rect,
                totalCount == 0 ? 0f : (float)currentCount / totalCount,
                $"{currentCount}/{totalCount}");

            EditorGUILayout.Space();

            // 現在処理中
            EditorGUILayout.LabelField("現在処理中", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(string.IsNullOrEmpty(current) ? "-" : current);

            EditorGUILayout.Space();

            // ログ
            EditorGUILayout.LabelField("ログ", EditorStyles.boldLabel);

            scroll = EditorGUILayout.BeginScrollView(
                scroll,
                GUILayout.ExpandHeight(true));

            foreach (var entry in logs)
            {
                if (entry.Type == LogType.Info)
                {
                    EditorGUILayout.LabelField(entry.Message); // HelpBoxより大幅に軽い
                }
                else
                {
                    var messageType = entry.Type == LogType.Warning ? MessageType.Warning : MessageType.Error;
                    EditorGUILayout.HelpBox(entry.Message, messageType);
                }
            }

            EditorGUILayout.EndScrollView();
            
            if (scrollToBottom)
            {
                scroll.y = float.MaxValue;
                scrollToBottom = false;
            }
            
            EditorGUI.BeginDisabledGroup(isRunning);

            if (GUILayout.Button("Close"))
            {
                Close();
            }

            EditorGUI.EndDisabledGroup();
        }
        
        /// <summary>
        /// ウィンドウを閉じるかどうかを判定する。実行中は閉じられない。
        /// </summary>
        /// <returns> 閉じて問題がない場合 true。 </returns>
        private bool WantsToClose()
        {
            if (isRunning)
            {
                EditorUtility.DisplayDialog("Drive Import", "同期完了までウィンドウを閉じられません。", "OK");
                return false;
            }
            return true;
        }
        
        /// <summary>
        /// ログエントリの種類。
        /// </summary>
        private enum LogType
        {
            /// <summary> 通常情報。 </summary>
            Info,
            /// <summary> 警告。 </summary>
            Warning,
            /// <summary> エラー。 </summary>
            Error,
        }

        /// <summary>
        /// ログエントリ。
        /// </summary>
        private struct LogEntry
        {
            /// <summary> ログの種類。 </summary>
            public LogType Type;
            /// <summary> ログメッセージ。 </summary>
            public string Message;

            public LogEntry(LogType type, string message)
            {
                Type = type;
                Message = message;
            }
        }
    }
}