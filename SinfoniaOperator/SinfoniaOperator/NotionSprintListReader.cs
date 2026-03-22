using Notion.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SinfoniaStudio.SinfoniaOperator
{
    internal class NotionSprintListReader
    {
        public NotionSprintListReader(NotionEnvironment env)
        {
            _reader = new NotionReader(env.NotionToken);
            _env = env;
        }

        public async Task<string> GetSprintContent()
        {
            try
            {
                List<IWikiDatabase> database = await _reader.GetDatabaseAsync(_env.SprintDatabaseID);

                // 日本時間を取得。
                DateTime nowTime = DateTime.UtcNow.AddHours(9);
                DateTime today = nowTime.Date;

                StringBuilder output = new();

                Console.WriteLine($"{new string('-', 5)}スプリントリスト ログ{new string('-', 5)}");

                foreach (IWikiDatabase item in database)
                {
                    if (item is not Page page) { continue; }

                    // 日付プロパティを取得できる場合。
                    if (!page.Properties.TryGetValue(_env.DatePropertyName, out PropertyValue? datePropertyValue) ||
                        datePropertyValue is not DatePropertyValue dateProperty) { continue; }

                    // ページ名を取得。
                    string pageName = NotionReader.GetPageName(page, _env.NamePropertyName);
                    DateTime startDate = default;
                    DateTime endDate = default;

                    if (!NotionReader.ConvertDateUtcToJst(dateProperty.Date.Start?.UtcDateTime, out startDate))
                    {
                        Console.WriteLine($"{pageName}は開始日時がないため、通知しません。");
                        continue; // 開始日時がない場合は、通知しない。
                    }

                    if (!NotionReader.ConvertDateUtcToJst(dateProperty.Date.End?.UtcDateTime, out endDate))
                    {
                        Console.WriteLine($"{pageName}は終了日時がないため、通知しません。");
                        continue; // 終了日時がない場合は、通知しない。
                    }
                    if (!(startDate <= today && today <= endDate))
                    {
                        Console.WriteLine($"{pageName}はスプリント期間外のため、通知しません。");
                        continue; // スプリント期間外の場合は、通知しない。
                    }

                    output.AppendLine($"# 今週のスプリント");
                    output.AppendLine($"タイトル【{pageName}】");
                    output.AppendLine($"開始日時: {startDate:yyyy/MM/dd HH:mm}");
                    output.AppendLine($"終了日時: {endDate:yyyy/MM/dd HH:mm}");

                    string pageContext = await _reader.GetPageContentAsync(page);
                    output.AppendLine(new string('-', 10));
                    output.AppendLine(pageContext.TrimEnd());
                    output.AppendLine(new string('-', 10));
                    output.AppendLine();
                    break;
                }

                return output.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"スプリントリストの取得に失敗しました: {ex}");
                return string.Empty;
            }
        }

        private readonly NotionReader _reader;
        private readonly NotionEnvironment _env;
    }
}
