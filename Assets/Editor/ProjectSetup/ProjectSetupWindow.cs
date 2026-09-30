using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace KillChord.Editor.ProjectSetup
{
    /// <summary>
    ///     初期セットアップの不足を知らせ、セットアップの CLI を起動するウィンドウ。
    /// </summary>
    internal sealed class ProjectSetupWindow : EditorWindow
    {
        /// <summary>
        ///     不足している項目を表示してウィンドウを開く。
        /// </summary>
        /// <param name="problems">不足している項目の説明。</param>
        public static void Open(IReadOnlyList<string> problems)
        {
            ProjectSetupWindow window = GetWindow<ProjectSetupWindow>(true, WINDOW_TITLE, true);
            window.minSize = new Vector2(WINDOW_WIDTH, WINDOW_HEIGHT);
            window.SetProblems(problems);
        }

        private const string WINDOW_TITLE = "プロジェクトのセットアップ";
        private const float WINDOW_WIDTH = 480f;
        private const float WINDOW_HEIGHT = 260f;

        private readonly List<string> _problems = new();
        private Vector2 _scrollPosition;

        /// <summary>
        ///     ウィンドウの中身を描く。
        /// </summary>
        private void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "このプロジェクトのセットアップが済んでいません。Setup.bat を実行してから、Unity を開き直してください。",
                MessageType.Warning);

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            foreach (string problem in _problems)
            {
                EditorGUILayout.LabelField($"・{problem}", EditorStyles.wordWrappedLabel);
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("セットアップを実行")) { RunSetup(); }
                if (GUILayout.Button("再確認")) { Recheck(); }
                if (GUILayout.Button("閉じる")) { Close(); }
            }
        }

        /// <summary>
        ///     表示する項目を差し替える。
        /// </summary>
        /// <param name="problems">不足している項目の説明。</param>
        private void SetProblems(IReadOnlyList<string> problems)
        {
            _problems.Clear();
            _problems.AddRange(problems);
            Repaint();
        }

        /// <summary>
        ///     Setup.bat を別のコンソールで起動する。
        /// </summary>
        private void RunSetup()
        {
            string batchPath = ProjectSetupChecker.SetupBatchPath;
            if (!File.Exists(batchPath))
            {
                Debug.LogError($"[{nameof(ProjectSetupWindow)}] {batchPath} が見つかりません。");
                return;
            }

            // 結果を読めるよう、Setup.bat の最後で止まる別ウィンドウで開く。
            ProcessStartInfo startInfo = new(batchPath)
            {
                WorkingDirectory = ProjectSetupChecker.RepositoryRoot,
                UseShellExecute = true
            };
            Process.Start(startInfo);
        }

        /// <summary>
        ///     もう一度調べ、不足が無くなっていれば閉じる。
        /// </summary>
        private void Recheck()
        {
            IReadOnlyList<string> problems = ProjectSetupChecker.FindProblems();
            if (problems.Count == 0)
            {
                Debug.Log($"[{nameof(ProjectSetupWindow)}] セットアップは完了しています。");
                Close();
                return;
            }

            SetProblems(problems);
        }
    }
}
