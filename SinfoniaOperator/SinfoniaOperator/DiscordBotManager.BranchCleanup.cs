using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SinfoniaStudio.SinfoniaOperator
{
    /// <summary>
    ///     削除可能なリモートブランチを一覧表示するスラッシュコマンドの実装。
    ///     Botのホストにリポジトリのクローンが無くても動くよう、GitHub APIを参照する。
    /// </summary>
    internal sealed partial class DiscordBotManager
    {
        /// <summary>
        ///     ブランチ整理コマンドに必要な依存情報を設定する。
        /// </summary>
        /// <param name="repository">"owner/name" 形式の調査対象リポジトリ。</param>
        /// <param name="accessToken">GitHubのアクセストークン。未指定でも動くがレート制限が厳しくなる。</param>
        /// <param name="guildId">コマンドを限定登録する任意のGuild ID。</param>
        public void ConfigureBranchCleanup(string repository, string? accessToken, ulong? guildId)
        {
            if (string.IsNullOrWhiteSpace(repository))
            {
                throw new ArgumentException("調査対象のリポジトリを指定してください。", nameof(repository));
            }

            _gitHubRepository = repository;
            _gitHubAccessToken = accessToken;
            _branchCleanupGuildId = guildId;
        }

        private const string BRANCHES_COMMAND_NAME = "branches";
        private const string BASE_OPTION_NAME = "base";
        private const int MAX_BRANCH_DISPLAY_COUNT = 25;

        /// <summary>
        ///     Discordへブランチ整理スラッシュコマンドを登録する。
        /// </summary>
        private async Task RegisterBranchesCommandAsync()
        {
            SlashCommandBuilder commandBuilder = new SlashCommandBuilder()
                .WithName(BRANCHES_COMMAND_NAME)
                .WithDescription("マージ済みで削除できるリモートブランチを一覧表示します。")
                .AddOption(
                    BASE_OPTION_NAME,
                    ApplicationCommandOptionType.String,
                    "マージ先として判定するブランチ名。既定はdevelop。",
                    isRequired: false);

            ApplicationCommandProperties command = commandBuilder.Build();
            if (_branchCleanupGuildId.HasValue)
            {
                await _client.Rest.CreateGuildCommand(command, _branchCleanupGuildId.Value);
                Console.WriteLine($"[DiscordBot] /branchesをGuild {_branchCleanupGuildId.Value} に登録しました。");
            }
            else
            {
                await _client.CreateGlobalApplicationCommandAsync(command);
                Console.WriteLine("[DiscordBot] /branchesをグローバルコマンドとして登録しました。");
            }
        }

        /// <summary>
        ///     ブランチ整理コマンドを処理し、結果をEmbedで返信する。
        /// </summary>
        /// <param name="command">受信したスラッシュコマンド。</param>
        private async Task HandleBranchesCommandAsync(SocketSlashCommand command)
        {
            // GitHub APIの往復が3秒を超えるため、先に応答を保留する。
            await command.DeferAsync();
            try
            {
                string repository = _gitHubRepository
                    ?? throw new InvalidOperationException("調査対象のGitHubリポジトリが設定されていません。");

                SocketSlashCommandDataOption? baseOption = command.Data.Options
                    .FirstOrDefault(option => string.Equals(option.Name, BASE_OPTION_NAME, StringComparison.Ordinal));
                string baseBranch = baseOption?.Value as string ?? GitRemoteBranchInspector.DEFAULT_BASE_BRANCH;

                using GitHubBranchInspector inspector = new(repository, _gitHubAccessToken);
                IReadOnlyList<RemoteBranchInfo> branches = await inspector.GetDeletableBranchesAsync(baseBranch);

                await command.FollowupAsync(embeds: [BuildBranchCleanupEmbed(repository, baseBranch, branches).Build()]);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DiscordBot] ブランチ一覧の取得に失敗しました: {ex.Message}");
                await command.FollowupAsync($"ブランチ一覧の取得に失敗しました: {Truncate(ex.Message, MAX_QUERY_DISPLAY_LENGTH)}");
            }
        }

        /// <summary>
        ///     削除可能なブランチの一覧をDiscord Embedへ変換する。
        /// </summary>
        /// <param name="repository">調査対象のリポジトリ。</param>
        /// <param name="baseBranch">マージ判定に使ったブランチ名。</param>
        /// <param name="branches">削除可能なブランチ。</param>
        /// <returns>一覧表示のEmbed構築器。</returns>
        private static EmbedBuilder BuildBranchCleanupEmbed(
            string repository,
            string baseBranch,
            IReadOnlyList<RemoteBranchInfo> branches)
        {
            EmbedBuilder builder = new EmbedBuilder()
                .WithTitle("削除可能なリモートブランチ")
                .WithColor(Color.Orange);

            if (branches.Count == 0)
            {
                builder.WithDescription($"`{repository}` に `{baseBranch}` へマージ済みのブランチはありません。");
                return builder;
            }

            StringBuilder description = new();
            description.AppendLine($"`{repository}` / ベース `{baseBranch}` / {branches.Count} 件（最終コミットが古い順）");
            description.AppendLine();

            int displayCount = Math.Min(branches.Count, MAX_BRANCH_DISPLAY_COUNT);
            for (int i = 0; i < displayCount; i++)
            {
                RemoteBranchInfo branch = branches[i];
                description.AppendLine(
                    $"`{branch.LastCommitDate.ToLocalTime():yyyy-MM-dd}` **{branch.Name}** — {branch.AuthorName} / {branch.Reason}");
            }

            if (branches.Count > displayCount)
            {
                description.AppendLine();
                description.AppendLine($"ほか {branches.Count - displayCount} 件は省略しました。`base` を絞るか、CLIの `branches --github` で全件を確認してください。");
            }

            builder.WithDescription(Truncate(description.ToString(), MAX_EMBED_DESCRIPTION_LENGTH));
            builder.WithFooter("削除は各自で `git push origin --delete <ブランチ名>` を実行してください。");
            return builder;
        }

        private string? _gitHubRepository;
        private string? _gitHubAccessToken;
        private ulong? _branchCleanupGuildId;
        private bool _isBranchesCommandRegistered;
    }
}
