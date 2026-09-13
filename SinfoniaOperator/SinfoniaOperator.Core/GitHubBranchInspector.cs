using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace SinfoniaStudio.SinfoniaOperator
{
    /// <summary>
    ///     GitHub APIを用いて、削除しても差分が失われないリモートブランチを調べるクラス。
    ///     ローカルにリポジトリのクローンが無い環境（常駐Botなど）でも利用できる。
    ///     squashマージやrebaseマージでコミットが書き換わったブランチも、
    ///     マージ済みPull Requestの情報から検出する。
    /// </summary>
    public sealed class GitHubBranchInspector : IDisposable
    {
        /// <summary>
        ///     対象リポジトリとアクセストークンを指定して生成する。
        /// </summary>
        /// <param name="repository">"owner/name" 形式のリポジトリ指定。</param>
        /// <param name="accessToken">GitHubのアクセストークン。未指定の場合は匿名アクセスになり、レート制限が厳しくなる。</param>
        public GitHubBranchInspector(string repository, string? accessToken)
        {
            if (string.IsNullOrWhiteSpace(repository) || !repository.Contains("/"))
            {
                throw new ArgumentException("リポジトリは owner/name 形式で指定してください。", nameof(repository));
            }

            _repository = repository.Trim().Trim('/');
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("SinfoniaOperator", "1.0"));
            _httpClient.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");

            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }
        }

        /// <summary>
        ///     ベースブランチへ取り込み済みで、削除可能なブランチを列挙する。
        /// </summary>
        /// <param name="baseBranch">マージ先として判定に使うブランチ名。</param>
        /// <param name="protectedBranches">削除対象から除外するブランチ名。nullの場合は既定値を使う。</param>
        /// <param name="cancellationToken">キャンセル用トークン。</param>
        /// <returns>最終コミットが古い順に並べた削除可能なブランチ。</returns>
        public async Task<IReadOnlyList<RemoteBranchInfo>> GetDeletableBranchesAsync(
            string baseBranch,
            IReadOnlyCollection<string>? protectedBranches = null,
            CancellationToken cancellationToken = default)
        {
            HashSet<string> protectedSet = new(
                protectedBranches ?? GitRemoteBranchInspector.DEFAULT_PROTECTED_BRANCHES,
                StringComparer.OrdinalIgnoreCase)
            {
                baseBranch
            };

            Dictionary<string, int> mergedPullRequestByHeadSha = await GetMergedPullRequestHeadsAsync(baseBranch, cancellationToken);
            JArray branches = await GetAllPagesAsync($"branches?per_page={PAGE_SIZE}", cancellationToken);

            List<RemoteBranchInfo> deletableBranches = new();
            foreach (JToken branchToken in branches)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string name = branchToken.Value<string>("name") ?? string.Empty;
                if (name.Length == 0 || protectedSet.Contains(name)) { continue; }

                // GitHubのブランチ保護が有効なブランチは削除できないため候補から外す。
                if (branchToken.Value<bool?>("protected") == true) { continue; }

                string headSha = branchToken["commit"]?.Value<string>("sha") ?? string.Empty;
                if (headSha.Length == 0) { continue; }

                string? reason = null;
                if (mergedPullRequestByHeadSha.TryGetValue(headSha, out int pullRequestNumber))
                {
                    reason = $"PR #{pullRequestNumber} マージ済み";
                }
                else if (await IsContainedInBaseAsync(baseBranch, name, cancellationToken))
                {
                    reason = $"{baseBranch} に取り込み済み";
                }

                if (reason == null) { continue; }

                (DateTimeOffset commitDate, string authorName) = await GetCommitSummaryAsync(headSha, cancellationToken);
                deletableBranches.Add(new RemoteBranchInfo(name, name, commitDate, authorName, reason));
            }

            deletableBranches.Sort(static (left, right) => left.LastCommitDate.CompareTo(right.LastCommitDate));
            return deletableBranches;
        }

        /// <summary>
        ///     HTTPクライアントを解放する。
        /// </summary>
        public void Dispose()
        {
            _httpClient.Dispose();
        }

        private const int PAGE_SIZE = 100;
        private const int MAXIMUM_PAGE_COUNT = 20;

        /// <summary>
        ///     指定ベースブランチへマージ済みのPull Requestについて、head側のコミットSHAと番号の対応を取得する。
        ///     squashマージやrebaseマージではブランチのコミットがベースに現れないため、この情報で補う。
        /// </summary>
        /// <param name="baseBranch">マージ先のブランチ名。</param>
        /// <param name="cancellationToken">キャンセル用トークン。</param>
        /// <returns>head側コミットSHAをキーとしたPull Request番号の辞書。</returns>
        private async Task<Dictionary<string, int>> GetMergedPullRequestHeadsAsync(
            string baseBranch,
            CancellationToken cancellationToken)
        {
            string path = $"pulls?state=closed&base={Uri.EscapeDataString(baseBranch)}&per_page={PAGE_SIZE}";
            JArray pullRequests = await GetAllPagesAsync(path, cancellationToken);

            Dictionary<string, int> mergedHeads = new(StringComparer.Ordinal);
            foreach (JToken pullRequest in pullRequests)
            {
                JToken? mergedAt = pullRequest["merged_at"];
                if (mergedAt == null || mergedAt.Type == JTokenType.Null) { continue; }

                string headSha = pullRequest["head"]?.Value<string>("sha") ?? string.Empty;
                int? number = pullRequest.Value<int?>("number");
                if (headSha.Length == 0 || number == null) { continue; }

                // 同じheadで複数のPRがある場合は、より新しい（番号が大きい）PRを採用する。
                if (!mergedHeads.TryGetValue(headSha, out int existingNumber) || number.Value > existingNumber)
                {
                    mergedHeads[headSha] = number.Value;
                }
            }

            return mergedHeads;
        }

        /// <summary>
        ///     ブランチのコミットがベースブランチへ完全に含まれているかを判定する。
        /// </summary>
        /// <param name="baseBranch">比較元のベースブランチ名。</param>
        /// <param name="branchName">判定するブランチ名。</param>
        /// <param name="cancellationToken">キャンセル用トークン。</param>
        /// <returns>ベースより進んだコミットが無い場合はtrue。</returns>
        private async Task<bool> IsContainedInBaseAsync(
            string baseBranch,
            string branchName,
            CancellationToken cancellationToken)
        {
            string path = $"compare/{Uri.EscapeDataString(baseBranch)}...{Uri.EscapeDataString(branchName)}";
            JObject comparison = await GetJsonAsync<JObject>(path, cancellationToken);
            return comparison.Value<int?>("ahead_by") == 0;
        }

        /// <summary>
        ///     コミットの日時と作成者名を取得する。
        /// </summary>
        /// <param name="sha">対象のコミットSHA。</param>
        /// <param name="cancellationToken">キャンセル用トークン。</param>
        /// <returns>コミット日時と作成者名。</returns>
        private async Task<(DateTimeOffset CommitDate, string AuthorName)> GetCommitSummaryAsync(
            string sha,
            CancellationToken cancellationToken)
        {
            JObject commit = await GetJsonAsync<JObject>($"commits/{sha}", cancellationToken);
            string authorName = commit["commit"]?["author"]?.Value<string>("name") ?? "unknown";

            DateTimeOffset commitDate = DateTimeOffset.MinValue;
            string? dateText = commit["commit"]?["committer"]?.Value<string>("date");
            if (!string.IsNullOrEmpty(dateText) && DateTimeOffset.TryParse(dateText, out DateTimeOffset parsedDate))
            {
                commitDate = parsedDate;
            }

            return (commitDate, authorName);
        }

        /// <summary>
        ///     ページングされたGitHub APIの全ページを連結して取得する。
        /// </summary>
        /// <param name="path">クエリ文字列を含むリポジトリ相対パス。</param>
        /// <param name="cancellationToken">キャンセル用トークン。</param>
        /// <returns>全ページの要素を連結した配列。</returns>
        private async Task<JArray> GetAllPagesAsync(string path, CancellationToken cancellationToken)
        {
            JArray allItems = new();
            string separator = path.Contains("?") ? "&" : "?";

            for (int page = 1; page <= MAXIMUM_PAGE_COUNT; page++)
            {
                JArray items = await GetJsonAsync<JArray>($"{path}{separator}page={page}", cancellationToken);
                foreach (JToken item in items)
                {
                    allItems.Add(item);
                }

                if (items.Count < PAGE_SIZE) { break; }
            }

            return allItems;
        }

        /// <summary>
        ///     GitHub APIを呼び出し、JSONを取得する。
        /// </summary>
        /// <typeparam name="TToken">期待するJSONトークンの型。</typeparam>
        /// <param name="path">リポジトリ相対のAPIパス。</param>
        /// <param name="cancellationToken">キャンセル用トークン。</param>
        /// <returns>解析済みのJSONトークン。</returns>
        private async Task<TToken> GetJsonAsync<TToken>(string path, CancellationToken cancellationToken)
            where TToken : JToken
        {
            string requestUri = $"https://api.github.com/repos/{_repository}/{path}";
            using HttpResponseMessage response = await _httpClient.GetAsync(requestUri, cancellationToken);
            string content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(BuildErrorMessage(response, content));
            }

            if (JToken.Parse(content) is not TToken token)
            {
                throw new InvalidOperationException($"GitHub APIの応答形式が想定と異なります: {path}");
            }

            return token;
        }

        /// <summary>
        ///     GitHub APIの失敗内容から、対処しやすいエラーメッセージを組み立てる。
        /// </summary>
        /// <param name="response">失敗したHTTP応答。</param>
        /// <param name="content">応答本文。</param>
        /// <returns>エラーメッセージ。</returns>
        private static string BuildErrorMessage(HttpResponseMessage response, string content)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return $"GitHubの認証に失敗しました。{OperatorConfigKeys.GITHUB_TOKEN} を確認してください。";
            }

            if (response.StatusCode == HttpStatusCode.Forbidden &&
                response.Headers.TryGetValues("x-ratelimit-remaining", out IEnumerable<string>? remainingValues))
            {
                foreach (string remaining in remainingValues)
                {
                    if (string.Equals(remaining, "0", StringComparison.Ordinal))
                    {
                        return $"GitHub APIのレート制限に達しました。{OperatorConfigKeys.GITHUB_TOKEN} を設定すると上限が緩和されます。";
                    }
                }
            }

            return $"GitHub APIの呼び出しに失敗しました ({(int)response.StatusCode}): {content}";
        }

        private readonly string _repository;
        private readonly HttpClient _httpClient;
    }
}
