using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace KillChord.Editor.SinfoniaOperator
{
    /// <summary>
    ///     エディタ起動時に、Notion仕様書のサブモジュール（Docs/NotionSpecifications）を裏で最新化するクラス。
    ///     仕様書はCIだけがNotionから取得して仕様書リポジトリへpushするため、各自はpullするだけでよい。
    /// </summary>
    [InitializeOnLoad]
    internal static class NotionSpecificationsAutoPull
    {
        /// <summary>
        ///     エディタ起動後の最初のドメインリロードでだけ、非同期でpullを開始する。
        /// </summary>
        static NotionSpecificationsAutoPull()
        {
            // バッチモード（自動ビルド）では取得しない。
            if (Application.isBatchMode) { return; }

            // ドメインリロードのたびに走らないよう、エディタ起動ごとに1回だけにする。
            if (SessionState.GetBool(SESSION_KEY, false)) { return; }
            SessionState.SetBool(SESSION_KEY, true);

            string repositoryRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            _ = Task.Run(() => PullSafely(repositoryRoot));
        }

        private const string SESSION_KEY = "KillChord.NotionSpecificationsAutoPull.HasRun";
        private const string SPECIFICATIONS_PATH = "Docs/NotionSpecifications";
        private const string BACKUP_SUFFIX_FORMAT = "yyyyMMddHHmmss";
        private const int TIMEOUT_MILLISECONDS = 5 * 60 * 1000;

        /// <summary>
        ///     pullを実行し、失敗してもエディタの作業を止めないよう例外をログへ流す。
        /// </summary>
        /// <param name="repositoryRoot">本体リポジトリのルート。</param>
        private static void PullSafely(string repositoryRoot)
        {
            try
            {
                Pull(repositoryRoot);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[{nameof(NotionSpecificationsAutoPull)}] 仕様書を更新できませんでした: {exception.Message}");
            }
        }

        /// <summary>
        ///     サブモジュールを取得し、仕様書リポジトリのmainの最新へ合わせる。
        /// </summary>
        /// <param name="repositoryRoot">本体リポジトリのルート。</param>
        private static void Pull(string repositoryRoot)
        {
            MoveLegacyExportAside(repositoryRoot);

            string before = RunGit(repositoryRoot, $"-C \"{SPECIFICATIONS_PATH}\" rev-parse --short HEAD", true).Output;
            GitResult result = RunGit(
                repositoryRoot,
                $"submodule update --init --remote --depth 1 -- \"{SPECIFICATIONS_PATH}\"",
                false);
            if (result.ExitCode != 0)
            {
                Debug.LogWarning($"[{nameof(NotionSpecificationsAutoPull)}] 仕様書を更新できませんでした。" +
                                 $"仕様書リポジトリへのアクセス権と、{SPECIFICATIONS_PATH} に手元の変更が残っていないかを確認してください。\n{result.Output}");
                return;
            }

            string after = RunGit(repositoryRoot, $"-C \"{SPECIFICATIONS_PATH}\" rev-parse --short HEAD", true).Output;
            if (before != after)
            {
                Debug.Log($"[{nameof(NotionSpecificationsAutoPull)}] 仕様書を最新に更新しました（{after}）。");
            }
        }

        /// <summary>
        ///     サブモジュール化する前に手元で書き出した仕様書があれば、消さずに別名で退避する。
        ///     中身が残っているとサブモジュールを取得できないため。
        /// </summary>
        /// <param name="repositoryRoot">本体リポジトリのルート。</param>
        private static void MoveLegacyExportAside(string repositoryRoot)
        {
            string directory = Path.Combine(repositoryRoot, SPECIFICATIONS_PATH);
            if (!Directory.Exists(directory)) { return; }

            // サブモジュールなら .git ファイルがある。空のディレクトリはgitがそのまま使える。
            bool isSubmodule = File.Exists(Path.Combine(directory, ".git"));
            bool isEmpty = Directory.GetFileSystemEntries(directory).Length == 0;
            if (isSubmodule || isEmpty) { return; }

            string backup = $"{directory}_old_{DateTime.Now.ToString(BACKUP_SUFFIX_FORMAT)}";
            Directory.Move(directory, backup);
            Debug.Log($"[{nameof(NotionSpecificationsAutoPull)}] 以前のエクスポート結果を {backup} へ退避しました。不要なら削除してください。");
        }

        /// <summary>
        ///     gitコマンドを実行し、終了コードと出力を返す。
        /// </summary>
        /// <param name="workingDirectory">実行するディレクトリ。</param>
        /// <param name="arguments">gitへ渡す引数。</param>
        /// <param name="isQuiet">失敗しても例外にしない問い合わせ用の実行かどうか。</param>
        /// <returns>実行結果。</returns>
        private static GitResult RunGit(string workingDirectory, string arguments, bool isQuiet)
        {
            ProcessStartInfo startInfo = new("git", arguments)
            {
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            // 認証情報の入力待ちで止まらないよう、対話プロンプトを無効にする。
            startInfo.EnvironmentVariables["GIT_TERMINAL_PROMPT"] = "0";

            using Process process = Process.Start(startInfo);
            if (process == null) { throw new InvalidOperationException("gitを起動できませんでした。"); }

            Task<string> standardOutput = process.StandardOutput.ReadToEndAsync();
            Task<string> standardError = process.StandardError.ReadToEndAsync();
            if (!process.WaitForExit(TIMEOUT_MILLISECONDS))
            {
                process.Kill();
                if (isQuiet) { return new GitResult(-1, string.Empty); }
                throw new TimeoutException("gitが時間内に終了しませんでした。");
            }

            string output = $"{standardOutput.Result}{standardError.Result}".Trim();
            return new GitResult(process.ExitCode, output);
        }

        /// <summary>
        ///     gitコマンドの実行結果。
        /// </summary>
        private readonly struct GitResult
        {
            /// <summary>
            ///     実行結果を生成する。
            /// </summary>
            /// <param name="exitCode">終了コード。</param>
            /// <param name="output">標準出力と標準エラー出力。</param>
            public GitResult(int exitCode, string output)
            {
                ExitCode = exitCode;
                Output = output;
            }

            /// <summary> 終了コード。 </summary>
            public int ExitCode { get; }

            /// <summary> 標準出力と標準エラー出力。 </summary>
            public string Output { get; }
        }
    }
}
