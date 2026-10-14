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
                $"このプロジェクトのセットアップが済んでいません。{ProjectSetupChecker.SetupEntryFileName} を実行してから、Unity を開き直してください。",
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
        ///     セットアップの入口（Setup.bat / Setup.command）を別のコンソールで起動する。
        /// </summary>
        private void RunSetup()
        {
            string entryPath = ProjectSetupChecker.SetupEntryPath;
            if (!File.Exists(entryPath))
            {
                Debug.LogError($"[{nameof(ProjectSetupWindow)}] {entryPath} が見つかりません。");
                return;
            }

            ProcessStartInfo startInfo = CreateSetupStartInfo(entryPath);
            if (startInfo == null)
            {
                Debug.LogWarning($"[{nameof(ProjectSetupWindow)}] この OS ではターミナルを自動で開けません。ターミナルから {entryPath} を実行してください。");
                return;
            }

            Process.Start(startInfo);
        }

        /// <summary>
        ///     結果を読めるよう、入口の最後で止まる別ウィンドウで開くための起動情報を作る。
        /// </summary>
        /// <param name="entryPath">セットアップの入口のパス。</param>
        /// <returns>起動情報。開くターミナルが決まらない OS（Linux）では null。</returns>
        private static ProcessStartInfo CreateSetupStartInfo(string entryPath)
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                    return new ProcessStartInfo(entryPath)
                    {
                        WorkingDirectory = ProjectSetupChecker.RepositoryRoot,
                        UseShellExecute = true
                    };
                case RuntimePlatform.OSXEditor:
                    // .command はターミナル.app で開くと、そのウィンドウで実行される。
                    return new ProcessStartInfo("open", $"-a Terminal \"{entryPath}\"")
                    {
                        WorkingDirectory = ProjectSetupChecker.RepositoryRoot,
                        UseShellExecute = false
                    };
                default:
                    return null;
            }
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
