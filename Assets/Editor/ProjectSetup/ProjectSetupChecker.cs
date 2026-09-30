using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace KillChord.Editor.ProjectSetup
{
    /// <summary>
    ///     エディタ起動時に、クローン後の初期セットアップが済んでいるかを確かめるクラス。
    ///     足りなければ <see cref="ProjectSetupWindow"/> で警告する。
    ///     セットアップの本体は Unity の外の CLI（Setup.bat）である。
    ///     他のアセンブリのコンパイルエラーに巻き込まれないよう、このアセンブリは何も参照しない。
    /// </summary>
    [InitializeOnLoad]
    internal static class ProjectSetupChecker
    {
        /// <summary>
        ///     エディタ起動後の最初のドメインリロードでだけ確認する。
        /// </summary>
        static ProjectSetupChecker()
        {
            // バッチモード（自動ビルド）ではウィンドウを出せないので確認しない。
            if (Application.isBatchMode) { return; }

            // ドメインリロードのたびに出ないよう、エディタ起動ごとに1回だけにする。
            if (SessionState.GetBool(SESSION_KEY, false)) { return; }
            SessionState.SetBool(SESSION_KEY, true);

            // 起動直後はレイアウトの復元中なので、落ち着いてからウィンドウを出す。
            EditorApplication.delayCall += ShowWindowIfNeeded;
        }

        /// <summary> リポジトリのルート。 </summary>
        public static string RepositoryRoot => Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

        /// <summary> ダブルクリックで実行するセットアップの入口。 </summary>
        public static string SetupBatchPath => Path.Combine(RepositoryRoot, "Setup.bat");

        /// <summary>
        ///     セットアップの不足を調べる。
        /// </summary>
        /// <returns>不足している項目の説明。無ければ空。</returns>
        public static IReadOnlyList<string> FindProblems()
        {
            List<string> problems = new();
            string root = RepositoryRoot;

            int requiredVersion = ReadSetupVersion(Path.Combine(root, CONFIG_RELATIVE_PATH));
            int completedVersion = ReadSetupVersion(Path.Combine(root, STATE_RELATIVE_PATH));
            if (completedVersion == 0)
            {
                problems.Add("セットアップ（Setup.bat）をまだ実行していません。");
            }
            else if (completedVersion < requiredVersion)
            {
                problems.Add($"セットアップの手順が更新されました（{completedVersion} → {requiredVersion}）。Setup.bat をもう一度実行してください。");
            }

            foreach (string submodulePath in ReadSubmodulePaths(Path.Combine(root, ".gitmodules")))
            {
                string directory = Path.Combine(root, submodulePath);
                if (!Directory.Exists(directory) || !Directory.EnumerateFileSystemEntries(directory).Any())
                {
                    problems.Add($"サブモジュール {submodulePath} を取得していません。");
                }
            }

            // ビルドターゲットが Android なので、ビルドしない役職でもモジュールが要る。
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
            {
                problems.Add("Android Build Support が入っていません。Unity Hub の「インストール」から、このバージョンにモジュールを追加してください。");
            }

            return problems;
        }

        private const string SESSION_KEY = "KillChord.ProjectSetupChecker.HasChecked";
        private const string CONFIG_RELATIVE_PATH = "PowerShell/ProjectSetup/project-setup.json";
        private const string STATE_RELATIVE_PATH = "UserSettings/KillChord/ProjectSetupState.json";
        private const string SUBMODULE_PATH_KEY = "path";

        /// <summary>
        ///     不足があれば警告ウィンドウを開く。
        /// </summary>
        private static void ShowWindowIfNeeded()
        {
            IReadOnlyList<string> problems = FindProblems();
            if (problems.Count == 0) { return; }

            Debug.LogWarning($"[{nameof(ProjectSetupChecker)}] プロジェクトのセットアップが済んでいません。\n{string.Join("\n", problems)}");
            ProjectSetupWindow.Open(problems);
        }

        /// <summary>
        ///     JSON ファイルから SetupVersion を読む。
        /// </summary>
        /// <param name="path">JSON ファイルのパス。</param>
        /// <returns>SetupVersion。ファイルが無い・読めない場合は0。</returns>
        private static int ReadSetupVersion(string path)
        {
            if (!File.Exists(path)) { return 0; }

            try
            {
                return JsonUtility.FromJson<SetupVersionJson>(File.ReadAllText(path)).SetupVersion;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        ///     .gitmodules からサブモジュールのパスを読む。
        /// </summary>
        /// <param name="path">.gitmodules のパス。</param>
        /// <returns>サブモジュールのパスの一覧。</returns>
        private static IEnumerable<string> ReadSubmodulePaths(string path)
        {
            if (!File.Exists(path)) { yield break; }

            foreach (string line in File.ReadAllLines(path))
            {
                string[] pair = line.Split('=');
                if (pair.Length != 2 || pair[0].Trim() != SUBMODULE_PATH_KEY) { continue; }

                yield return pair[1].Trim();
            }
        }

        /// <summary>
        ///     project-setup.json と ProjectSetupState.json の共通部分。
        /// </summary>
        [Serializable]
        private sealed class SetupVersionJson
        {
            /// <summary> セットアップ手順のバージョン。JsonUtility で読むため公開フィールドにする。 </summary>
            public int SetupVersion = 0;
        }
    }
}
