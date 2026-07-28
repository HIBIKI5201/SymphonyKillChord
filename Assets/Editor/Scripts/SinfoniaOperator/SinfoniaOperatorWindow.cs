using Cysharp.Threading.Tasks;
using KillChord.Editor.Utility;
using SinfoniaStudio.SinfoniaOperator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Editor.SinfoniaOperator
{
    /// <summary>
    ///     Notionのタスク表の表示と、Discordへの作業ログ送信を行うエディタウィンドウ。
    ///     Notion/Discordの設定はBot本体(Exe)と共有する公開・秘密JSON設定ファイルから読み込む。
    ///     ファイルへのパスは [Edit > Project Settings > KillChord > SinfoniaOperator] で設定する。
    /// </summary>
    public sealed class SinfoniaOperatorWindow : EditorWindow
    {
        [MenuItem(EditorWindowPathConst.SINFONIA_OPERATOR_PATH)]
        private static void Open()
        {
            SinfoniaOperatorWindow window = GetWindow<SinfoniaOperatorWindow>();
            window.titleContent = new GUIContent("Sinfonia Operator");
            window.minSize = new Vector2(560, 400);
        }

        private const float LIST_ITEM_HEIGHT = 22f;
        private const float WORK_LOG_FIELD_HEIGHT = 60f;

        private readonly List<NotionTaskItem> _items = new();

        private MultiColumnListView _taskListView;
        private Button _refreshButton;
        private Button _sendButton;
        private TextField _workLogField;
        private Label _statusLabel;

        private void CreateGUI()
        {
            rootVisualElement.Clear();

            // ヘッダー（更新ボタンとステータス表示）。
            VisualElement header = new()
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    marginTop = 4, marginBottom = 4, marginLeft = 4, marginRight = 4,
                }
            };
            _refreshButton = new Button(() => RefreshTasksAsync().Forget()) { text = "タスク表を更新" };
            _statusLabel = new Label(string.Empty) { style = { marginLeft = 8 } };
            header.Add(_refreshButton);
            header.Add(_statusLabel);
            rootVisualElement.Add(header);

            // タスク表。
            _taskListView = CreateTaskListView();
            rootVisualElement.Add(_taskListView);

            // 作業ログ送信エリア。
            rootVisualElement.Add(CreateWorkLogArea());
        }

        /// <summary>
        ///     Notionからタスク一覧を取得してタスク表を更新する。
        /// </summary>
        /// <returns></returns>
        private async UniTaskVoid RefreshTasksAsync()
        {
            if (!EnsureConfigLoaded()) { return; }

            _refreshButton.SetEnabled(false);
            _statusLabel.text = "取得中...";

            try
            {
                NotionEnvironment env = NotionEnvironment.FromConfig(
                    OperatorConfigKeys.NOTION_TOKEN,
                    OperatorConfigKeys.NOTION_TASK_DATABASE_ID,
                    OperatorConfigKeys.NOTION_SPRINT_DATABASE_ID,
                    OperatorConfigKeys.NOTION_DATABASE_DATE_PROPERTY,
                    OperatorConfigKeys.NOTION_DATABASE_NAME_PROPERTY,
                    OperatorConfigKeys.NOTION_DATABASE_STATUS_PROPERTY,
                    OperatorConfigKeys.NOTION_DATABASE_STATUS_TASK_DONE_PROPERTY);

                NotionTaskListReader reader = new(env);
                List<NotionTaskItem> items = await reader.GetTaskItemsAsync();

                // 通知区分の重要度順、次に締切の近い順で並べる。
                _items.Clear();
                _items.AddRange(items
                    .OrderByDescending(GetSortPriority)
                    .ThenBy(item => item.HasDate ? item.EndDate : DateTime.MaxValue));

                _taskListView.RefreshItems();
                _statusLabel.text = $"{_items.Count} 件 (更新: {DateTime.Now:HH:mm:ss})";
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{nameof(SinfoniaOperatorWindow)}] タスク一覧の取得に失敗しました: {ex.Message}\n" +
                    "JSON設定ファイルの内容を確認してください。");
                _statusLabel.text = "取得に失敗しました";
            }
            finally
            {
                _refreshButton.SetEnabled(true);
            }
        }

        /// <summary>
        ///     入力された作業ログをDiscordのWebhookへ送信する。
        /// </summary>
        /// <returns></returns>
        private async UniTaskVoid SendWorkLogAsync()
        {
            if (!EnsureConfigLoaded()) { return; }

            string message = _workLogField.value;
            if (string.IsNullOrWhiteSpace(message))
            {
                _statusLabel.text = "作業ログが空です";
                return;
            }

            string botToken = OperatorConfig.GetValue(OperatorConfigKeys.DISCORD_BOT_TOKEN);
            string webhookUrl = OperatorConfig.GetValue(OperatorConfigKeys.DISCORD_WEBHOOK_URL);
            string workLogChannelIdRaw = OperatorConfig.GetValue(OperatorConfigKeys.DISCORD_WORK_LOG_CHANNEL_ID);

            bool useBot = !string.IsNullOrWhiteSpace(botToken);
            bool hasChannelId = ulong.TryParse(workLogChannelIdRaw?.Trim(), out ulong channelId);
            if (useBot && !hasChannelId)
            {
                Debug.LogError($"[{nameof(SinfoniaOperatorWindow)}] {OperatorConfigKeys.DISCORD_WORK_LOG_CHANNEL_ID} が未設定か、数値として解釈できません。" +
                    "JSON設定ファイルを確認してください。");
                _statusLabel.text = "設定が不足しています";
                return;
            }
            else if (!useBot && string.IsNullOrWhiteSpace(webhookUrl))
            {
                Debug.LogError($"[{nameof(SinfoniaOperatorWindow)}] {OperatorConfigKeys.DISCORD_BOT_TOKEN} または " +
                    $"{OperatorConfigKeys.DISCORD_WEBHOOK_URL} が未設定です。JSON設定ファイルを確認してください。");
                _statusLabel.text = "設定が不足しています";
                return;
            }

            _sendButton.SetEnabled(false);
            _statusLabel.text = "送信中...";

            try
            {
                SinfoniaOperatorSettings settings = SinfoniaOperatorSettings.instance;
                string userName = string.IsNullOrWhiteSpace(settings.WorkLogUserName)
                    ? Environment.UserName
                    : settings.WorkLogUserName;
                string content = $"**[作業ログ] {userName}** {DateTimeUtility.JstNow():yyyy/MM/dd HH:mm}\n{message}";

                // Botトークンが設定されていればボットアカウントとして送信し、なければWebhookで送信する。
                bool isSucceeded;
                if (useBot)
                {
                    DiscordBotRestClient client = new(botToken);
                    isSucceeded = await client.SendMessageAsync(channelId, content);
                }
                else
                {
                    DiscordWebhookClient client = new(webhookUrl);
                    isSucceeded = await client.SendMessageAsync(content);
                }

                if (isSucceeded)
                {
                    _workLogField.SetValueWithoutNotify(string.Empty);
                    _statusLabel.text = "作業ログを送信しました";
                }
                else
                {
                    Debug.LogError($"[{nameof(SinfoniaOperatorWindow)}] 作業ログの送信に失敗しました。コンソールのログを確認してください。");
                    _statusLabel.text = "送信に失敗しました";
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{nameof(SinfoniaOperatorWindow)}] 作業ログの送信中にエラーが発生しました: {ex.Message}");
                _statusLabel.text = "送信に失敗しました";
            }
            finally
            {
                _sendButton.SetEnabled(true);
            }
        }

        /// <summary>
        ///     タスク表のMultiColumnListViewを構築する。
        /// </summary>
        /// <returns></returns>
        private MultiColumnListView CreateTaskListView()
        {
            Columns columns = new();

            columns.Add(new Column
            {
                name = "category",
                title = "区分",
                width = 100,
                makeCell = () => new Label(),
                bindCell = (element, index) => ((Label)element).text = GetCategoryLabel(_items[index].Category),
            });
            columns.Add(new Column
            {
                name = "name",
                title = "タスク名",
                minWidth = 160,
                stretchable = true,
                makeCell = () => new Label { style = { unityTextOverflowPosition = TextOverflowPosition.End } },
                bindCell = (element, index) => ((Label)element).text = _items[index].Name,
            });
            columns.Add(new Column
            {
                name = "status",
                title = "ステータス",
                width = 90,
                makeCell = () => new Label(),
                bindCell = (element, index) => ((Label)element).text = _items[index].Status,
            });
            columns.Add(new Column
            {
                name = "start",
                title = "開始日",
                width = 80,
                makeCell = () => new Label(),
                bindCell = (element, index) => ((Label)element).text = FormatDate(_items[index], _items[index].StartDate),
            });
            columns.Add(new Column
            {
                name = "end",
                title = "締切",
                width = 80,
                makeCell = () => new Label(),
                bindCell = (element, index) => ((Label)element).text = FormatDate(_items[index], _items[index].EndDate),
            });
            columns.Add(new Column
            {
                name = "open",
                title = string.Empty,
                width = 60,
                makeCell = () => new Button { text = "開く" },
                bindCell = (element, index) =>
                {
                    Button button = (Button)element;
                    NotionTaskItem item = _items[index];
                    // バインドのたびにClickableを差し替えて、ハンドラの多重登録を防ぐ。
                    button.clickable = new Clickable(() => OpenTaskPage(item));
                    button.SetEnabled(!string.IsNullOrEmpty(item.Url) || !string.IsNullOrEmpty(item.PublicUrl));
                },
            });

            return new MultiColumnListView(columns)
            {
                itemsSource = _items,
                fixedItemHeight = LIST_ITEM_HEIGHT,
                style = { flexGrow = 1 },
            };
        }

        /// <summary>
        ///     作業ログの入力欄と送信ボタンを構築する。
        /// </summary>
        /// <returns></returns>
        private VisualElement CreateWorkLogArea()
        {
            VisualElement area = new()
            {
                style = { marginTop = 4, marginBottom = 4, marginLeft = 4, marginRight = 4 }
            };

            area.Add(new Label("作業ログ") { style = { unityFontStyleAndWeight = FontStyle.Bold } });

            _workLogField = new TextField { multiline = true };
            _workLogField.style.height = WORK_LOG_FIELD_HEIGHT;
            _workLogField.style.whiteSpace = WhiteSpace.Normal;
            area.Add(_workLogField);

            _sendButton = new Button(() => SendWorkLogAsync().Forget()) { text = "Discordへ送信" };
            _sendButton.style.alignSelf = Align.FlexEnd;
            _sendButton.style.marginTop = 2;
            area.Add(_sendButton);

            return area;
        }

        /// <summary>
        ///     Bot本体(Exe)と共有する公開・秘密JSON設定ファイルを読み込む。
        ///     公開設定が見つからない場合はエラーを表示してfalseを返す。
        /// </summary>
        /// <returns></returns>
        private bool EnsureConfigLoaded()
        {
            SinfoniaOperatorSettings settings = SinfoniaOperatorSettings.instance;
            string environmentPath = ResolveConfigPath(
                settings.EnvironmentConfigJsonPath,
                SinfoniaOperatorSettings.DEFAULT_ENVIRONMENT_CONFIG_JSON_PATH);
            string secretsPath = ResolveConfigPath(
                settings.SecretsConfigJsonPath,
                SinfoniaOperatorSettings.DEFAULT_SECRETS_CONFIG_JSON_PATH);
            string legacyPath = ResolveConfigPath(
                SinfoniaOperatorSettings.LEGACY_CONFIG_JSON_PATH,
                SinfoniaOperatorSettings.LEGACY_CONFIG_JSON_PATH);

            OperatorConfig.ClearOverrides();
            if (!OperatorConfig.LoadJsonFile(environmentPath))
            {
                if (OperatorConfig.LoadJsonFile(legacyPath))
                {
                    Debug.LogWarning($"[{nameof(SinfoniaOperatorWindow)}] 分割前の設定ファイルを読み込みました。" +
                        $"{SinfoniaOperatorSettings.DEFAULT_SECRETS_CONFIG_JSON_PATH}への移行を推奨します。");
                    return true;
                }

                Debug.LogError($"[{nameof(SinfoniaOperatorWindow)}] 公開JSON設定ファイルが見つかりません: {environmentPath}\n" +
                    "リポジトリから設定ファイルを取得したか確認してください。");
                _statusLabel.text = "公開設定が見つかりません";
                return false;
            }

            if (OperatorConfig.LoadJsonFile(secretsPath)) { return true; }

            if (OperatorConfig.LoadJsonFile(
                    legacyPath,
                    OperatorConfigKeys.DISCORD_BOT_TOKEN,
                    OperatorConfigKeys.NOTION_TOKEN))
            {
                Debug.LogWarning($"[{nameof(SinfoniaOperatorWindow)}] トークンを旧設定ファイルから読み込みました。" +
                    $"{SinfoniaOperatorSettings.DEFAULT_SECRETS_CONFIG_JSON_PATH}への移行を推奨します。");
                return true;
            }

            Debug.LogError($"[{nameof(SinfoniaOperatorWindow)}] 秘密JSON設定ファイルが見つかりません: {secretsPath}\n" +
                "sinfonia-operator.secrets.sample.jsonをコピーし、NotionとDiscordのトークンを設定してください。");
            _statusLabel.text = "秘密設定が見つかりません";
            return false;
        }

        /// <summary>
        ///     設定されたJSON設定ファイルのパスを絶対パスへ解決する。
        ///     相対パスの場合はプロジェクトルート（Assetsフォルダの親）を基準とする。
        /// </summary>
        /// <param name="configuredPath">設定されたパス。</param>
        /// <param name="defaultPath">未設定時に使用するパス。</param>
        /// <returns></returns>
        private static string ResolveConfigPath(string configuredPath, string defaultPath)
        {
            string path = string.IsNullOrWhiteSpace(configuredPath) ? defaultPath : configuredPath;
            if (path == SinfoniaOperatorSettings.LEGACY_CONFIG_JSON_PATH &&
                defaultPath != SinfoniaOperatorSettings.LEGACY_CONFIG_JSON_PATH)
            {
                path = defaultPath;
            }

            if (Path.IsPathRooted(path)) { return path; }

            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            return Path.Combine(projectRoot, path);
        }

        /// <summary>
        ///     タスクのNotionページをブラウザで開く。
        /// </summary>
        /// <param name="item"></param>
        private static void OpenTaskPage(NotionTaskItem item)
        {
            string url = !string.IsNullOrEmpty(item.Url) ? item.Url : item.PublicUrl;
            if (string.IsNullOrEmpty(url)) { return; }

            Application.OpenURL(url);
        }

        /// <summary>
        ///     並び替え用の重要度を取得する。
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private static int GetSortPriority(NotionTaskItem item)
        {
            return item.Category switch
            {
                NotionTaskCategory.Overdue => 4,
                NotionTaskCategory.Deadline => 3,
                NotionTaskCategory.Start => 2,
                NotionTaskCategory.None => 1,
                NotionTaskCategory.Done => 0,
                _ => 0,
            };
        }

        /// <summary>
        ///     通知区分の表示文字列を取得する。
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        private static string GetCategoryLabel(NotionTaskCategory category)
        {
            return category switch
            {
                NotionTaskCategory.Overdue => "🔴 納期遅れ",
                NotionTaskCategory.Deadline => "🟡 本日納期",
                NotionTaskCategory.Start => "🟢 本日開始",
                NotionTaskCategory.Done => "✅ 完了",
                _ => string.Empty,
            };
        }

        /// <summary>
        ///     日付の表示文字列を取得する。日付未設定の場合は「-」を返す。
        /// </summary>
        /// <param name="item"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        private static string FormatDate(NotionTaskItem item, DateTime date)
        {
            return item.HasDate ? date.ToString("MM/dd") : "-";
        }
    }
}
