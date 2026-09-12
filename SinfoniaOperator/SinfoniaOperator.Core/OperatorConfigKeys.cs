namespace SinfoniaStudio.SinfoniaOperator
{
    /// <summary>
    ///     OperatorConfig（JSON設定・環境変数）で使用するキー名の定数。
    ///     Bot本体(Exe)とUnityエディタの両方から同じキー名で同じ設定を参照するための共通定義。
    /// </summary>
    public static class OperatorConfigKeys
    {
        public const string DISCORD_BOT_TOKEN = "DISCORD_BOT_TOKEN";
        public const string DISCORD_TASK_CHANNEL_ID = "DISCORD_TASK_CHANNEL_ID";
        public const string DISCORD_TASK_ALERT_CHANNEL_ID = "DISCORD_TASK_ALERT_CHANNEL_ID";
        public const string DISCORD_SPRINT_CHANNEL_ID = "DISCORD_SPRINT_CHANNEL_ID";

        /// <summary> Unityエディタの作業ログ機能が送信先とするチャンネルID。 </summary>
        public const string DISCORD_WORK_LOG_CHANNEL_ID = "DISCORD_WORK_LOG_CHANNEL_ID";

        /// <summary> BotトークンでのREST送信の代わりに使用するWebhook URL。 </summary>
        public const string DISCORD_WEBHOOK_URL = "DISCORD_WEBHOOK_URL";

        /// <summary> Discordログの取得対象にするチャンネルID配列。 </summary>
        public const string DISCORD_LOG_CHANNEL_IDS = "DISCORD_LOG_CHANNEL_IDS";

        public const string NOTION_TOKEN = "NOTION_TOKEN";
        public const string NOTION_TASK_DATABASE_ID = "NOTION_TASK_DATABASE_ID";
        public const string NOTION_SPRINT_DATABASE_ID = "NOTION_SPRINT_DATABASE_ID";
        public const string NOTION_DATABASE_DATE_PROPERTY = "NOTION_DATABASE_DATE_PROPERTY";
        public const string NOTION_DATABASE_NAME_PROPERTY = "NOTION_DATABASE_NAME_PROPERTY";
        public const string NOTION_DATABASE_STATUS_PROPERTY = "NOTION_DATABASE_STATUS_PROPERTY";
        public const string NOTION_DATABASE_STATUS_TASK_DONE_PROPERTY = "NOTION_DATABASE_STATUS_TASK_DONE_PROPERTY";

        /// <summary> Markdownエクスポートの起点にするNotionページのURLまたはID。 </summary>
        public const string NOTION_EXPORT_ROOT_PAGE = "NOTION_EXPORT_ROOT_PAGE";

        /// <summary> Markdownエクスポートの出力先ディレクトリ。 </summary>
        public const string NOTION_EXPORT_OUTPUT = "NOTION_EXPORT_OUTPUT";

        /// <summary>
        ///     Markdown書き込みを許可するページIDの配列。
        ///     ここに挙げたページの子孫だけが編集対象になる。
        /// </summary>
        public const string NOTION_WRITE_ALLOWED_ROOTS = "NOTION_WRITE_ALLOWED_ROOTS";
    }
}
