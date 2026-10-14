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
    ///     セットアップの本体は Unity の外の CLI（Windows は Setup.bat、macOS / Linux は Setup.command）である。
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

        /// <summary> ダブルクリックで実行するセットアップの入口のファイル名（実行中の OS 向け）。 </summary>
        public static string SetupEntryFileName =>
            Application.platform == RuntimePlatform.WindowsEditor ? WINDOWS_SETUP_FILE_NAME : UNIX_SETUP_FILE_NAME;

        /// <summary> ダブルクリックで実行するセットアップの入口。 </summary>
        public static string SetupEntryPath => Path.Combine(RepositoryRoot, SetupEntryFileName);

        /// <summary>
        ///     セットアップの不足を調べる。
        /// </summary>
        /// <returns>不足している項目の説明。無ければ空。</returns>
        public static IReadOnlyList<string> FindProblems()
        {
            List<string> problems = new();
            string root = RepositoryRoot;

            SetupJson config = ReadSetupJson(Path.Combine(root, CONFIG_RELATIVE_PATH));
            int requiredVersion = config.SetupVersion;
            int completedVersion = ReadSetupJson(Path.Combine(root, STATE_RELATIVE_PATH)).SetupVersion;
            if (completedVersion == 0)
            {
                problems.Add($"セットアップ（{SetupEntryFileName}）をまだ実行していません。");
            }
            else if (completedVersion < requiredVersion)
            {
                problems.Add($"セットアップの手順が更新されました（{completedVersion} → {requiredVersion}）。{SetupEntryFileName} をもう一度実行してください。");
            }

            // Unity と IDE はフォルダ名からソリューションを作るので、名前が違うと別名の .slnx ができて二重になる。
            string folderName = Path.GetFileName(root);
            if (!string.IsNullOrEmpty(config.RepositoryFolderName) &&
                !string.Equals(folderName, config.RepositoryFolderName, StringComparison.Ordinal))
            {
                problems.Add($"クローン先のフォルダ名が「{folderName}」です。Unity と IDE を閉じてから「{config.RepositoryFolderName}」に変えてください。");
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
        private const string WINDOWS_SETUP_FILE_NAME = "Setup.bat";
        private const string UNIX_SETUP_FILE_NAME = "Setup.command";
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
        ///     project-setup.json か ProjectSetupState.json を読む。
        /// </summary>
        /// <param name="path">JSON ファイルのパス。</param>
        /// <returns>読んだ内容。ファイルが無い・読めない場合は既定値（SetupVersion が0）。</returns>
        private static SetupJson ReadSetupJson(string path)
        {
            if (!File.Exists(path)) { return new SetupJson(); }

            try
            {
                return JsonUtility.FromJson<SetupJson>(File.ReadAllText(path)) ?? new SetupJson();
            }
            catch (Exception)
            {
                return new SetupJson();
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
        private sealed class SetupJson
        {
            /// <summary> セットアップ手順のバージョン。JsonUtility で読むため公開フィールドにする。 </summary>
            public int SetupVersion = 0;

            /// <summary> クローン先のフォルダ名（project-setup.json だけが持つ）。 </summary>
            public string RepositoryFolderName = string.Empty;
        }
    }
}
