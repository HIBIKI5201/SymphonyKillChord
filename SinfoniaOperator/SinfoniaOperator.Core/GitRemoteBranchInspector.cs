using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace SinfoniaStudio.SinfoniaOperator
{
    /// <summary>
    ///     リモートブランチの1件分の情報。
    /// </summary>
    public readonly struct RemoteBranchInfo
    {
        /// <summary>
        ///     各値を指定して生成する。
        /// </summary>
        /// <param name="name">リモート名を除いたブランチ名。</param>
        /// <param name="fullName">リモート名を含むブランチ名。</param>
        /// <param name="lastCommitDate">最終コミット日時。</param>
        /// <param name="authorName">最終コミットの作成者名。</param>
        /// <param name="reason">削除可能と判定した理由。</param>
        public RemoteBranchInfo(
            string name,
            string fullName,
            DateTimeOffset lastCommitDate,
            string authorName,
            string reason)
        {
            Name = name;
            FullName = fullName;
            LastCommitDate = lastCommitDate;
            AuthorName = authorName;
            Reason = reason;
        }

        /// <summary> リモート名を除いたブランチ名。 </summary>
        public string Name { get; }

        /// <summary> リモート名を含むブランチ名。 </summary>
        public string FullName { get; }

        /// <summary> 最終コミット日時。 </summary>
        public DateTimeOffset LastCommitDate { get; }

        /// <summary> 最終コミットの作成者名。 </summary>
        public string AuthorName { get; }

        /// <summary> 削除可能と判定した理由。 </summary>
        public string Reason { get; }
    }

    /// <summary>
    ///     gitコマンドを実行し、ベースブランチへマージ済みで削除可能なリモートブランチを調べるクラス。
    ///     保護対象ブランチとベースブランチ自身は結果から除外する。
    /// </summary>
    public sealed class GitRemoteBranchInspector
    {
        /// <summary>
        ///     削除対象から常に除外するブランチ名。
        ///     このリポジトリの長期ブランチは develop と main のため、masterは保護しない。
        /// </summary>
        public static readonly string[] DEFAULT_PROTECTED_BRANCHES = { "develop", "main", "HEAD" };

        /// <summary> 既定のリモート名。 </summary>
        public const string DEFAULT_REMOTE_NAME = "origin";

        /// <summary> 既定のベースブランチ名。 </summary>
        public const string DEFAULT_BASE_BRANCH = "develop";

        /// <summary> git上でベースブランチへマージ済みと判定した場合の理由。 </summary>
        private const string MERGED_REASON = "ベースへマージ済み";

        /// <summary>
        ///     リポジトリの作業ディレクトリを指定して生成する。
        /// </summary>
        /// <param name="repositoryRoot">gitコマンドを実行するディレクトリ。</param>
        public GitRemoteBranchInspector(string repositoryRoot)
        {
            if (string.IsNullOrWhiteSpace(repositoryRoot))
            {
                throw new ArgumentException("リポジトリのパスが空です。", nameof(repositoryRoot));
            }

            _repositoryRoot = Path.GetFullPath(repositoryRoot);
        }

        /// <summary>
        ///     リモートの削除済みブランチをローカルの追跡参照から取り除き、最新の状態を取得する。
        /// </summary>
        /// <param name="remoteName">対象のリモート名。</param>
        public void FetchAndPrune(string remoteName)
        {
            RunGit("fetch", "--prune", remoteName);
        }

        /// <summary>
        ///     ベースブランチへマージ済みで、削除しても差分が失われないリモートブランチを列挙する。
        /// </summary>
        /// <param name="remoteName">対象のリモート名。</param>
        /// <param name="baseBranch">マージ先として判定に使うブランチ名。</param>
        /// <param name="protectedBranches">削除対象から除外するブランチ名。nullの場合は既定値を使う。</param>
        /// <returns>最終コミットが古い順に並べた削除可能なブランチ。</returns>
        public IReadOnlyList<RemoteBranchInfo> GetDeletableBranches(
            string remoteName,
            string baseBranch,
            IReadOnlyCollection<string>? protectedBranches = null)
        {
            string baseRef = $"{remoteName}/{baseBranch}";
            HashSet<string> protectedSet = new(protectedBranches ?? DEFAULT_PROTECTED_BRANCHES, StringComparer.OrdinalIgnoreCase)
            {
                baseBranch
            };

            HashSet<string> mergedFullNames = new(StringComparer.Ordinal);
            foreach (string line in RunGit("branch", "--remotes", "--merged", baseRef).Split('\n'))
            {
                string trimmed = line.Trim();

                // "origin/HEAD -> origin/develop" のようなシンボリック参照は実体ではないため除外する。
                if (trimmed.Length == 0 || trimmed.Contains("->")) { continue; }

                mergedFullNames.Add(trimmed);
            }

            List<RemoteBranchInfo> branches = new();
            string format = "%(refname:short)%09%(committerdate:iso-strict)%09%(authorname)";
            foreach (string line in RunGit("for-each-ref", "--format=" + format, $"refs/remotes/{remoteName}").Split('\n'))
            {
                string trimmed = line.TrimEnd('\r');
                if (trimmed.Length == 0) { continue; }

                string[] columns = trimmed.Split('\t');
                if (columns.Length < 3) { continue; }

                string fullName = columns[0];
                if (!mergedFullNames.Contains(fullName)) { continue; }

                string shortName = StripRemotePrefix(fullName, remoteName);
                if (protectedSet.Contains(shortName)) { continue; }

                if (!DateTimeOffset.TryParse(columns[1], out DateTimeOffset committerDate))
                {
                    committerDate = DateTimeOffset.MinValue;
                }

                branches.Add(new RemoteBranchInfo(shortName, fullName, committerDate, columns[2], MERGED_REASON));
            }

            branches.Sort(static (left, right) => left.LastCommitDate.CompareTo(right.LastCommitDate));
            return branches;
        }

        /// <summary>
        ///     "origin/feature/x" から先頭のリモート名を取り除く。
        /// </summary>
        /// <param name="fullName">リモート名を含むブランチ名。</param>
        /// <param name="remoteName">リモート名。</param>
        /// <returns>リモート名を除いたブランチ名。</returns>
        private static string StripRemotePrefix(string fullName, string remoteName)
        {
            string prefix = remoteName + "/";
            return fullName.StartsWith(prefix, StringComparison.Ordinal)
                ? fullName[prefix.Length..]
                : fullName;
        }

        /// <summary>
        ///     gitコマンドを同期実行し、標準出力を取得する。
        /// </summary>
        /// <param name="arguments">gitへ渡す引数。</param>
        /// <returns>標準出力の内容。</returns>
        private string RunGit(params string[] arguments)
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = "git",
                WorkingDirectory = _repositoryRoot,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            foreach (string argument in arguments)
            {
                startInfo.ArgumentList.Add(argument);
            }

            using Process? process = Process.Start(startInfo)
                ?? throw new InvalidOperationException("gitプロセスの起動に失敗しました。gitにPATHが通っているか確認してください。");

            string standardOutput = process.StandardOutput.ReadToEnd();
            string standardError = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                string commandLine = "git " + string.Join(' ', arguments);
                throw new InvalidOperationException($"{commandLine} が失敗しました (exit {process.ExitCode}): {standardError.Trim()}");
            }

            return standardOutput;
        }

        private readonly string _repositoryRoot;
    }
}
