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
            StringBuilder logBuilder = new();
            StringBuilder output = new();
            try
            {
                logBuilder.AppendLine($"[SprintReader] スプリント情報の構築を開始します。 (Target Property: {_env.DatePropertyName})");
                List<IWikiDatabase> database = await _reader.GetDatabaseAsync(_env.SprintDatabaseID);

                // 日本時間を取得。
                DateTime nowTime = DateTimeUtility.JstNow();
                DateTime today = nowTime.Date;
                logBuilder.AppendLine($"[SprintReader] 判定基準日 (JST): {today:yyyy/MM/dd}");

                int evaluatedCount = 0;
                int matchCount = 0;

                output.AppendLine($"{new string('-', 5)}スプリントリスト ログ{new string('-', 5)}");

                foreach (IWikiDatabase item in database)
                {
                    evaluatedCount++;
                    if (item == null) continue;

                    if (item is not Page page) 
                    {
                        logBuilder.AppendLine($"[SprintReader] アイテム {evaluatedCount} はページではないためスキップします。");
                        continue; 
                    }

                    string pageName = NotionReader.GetPageName(page, _env.NamePropertyName) ?? "(null)";

                    // 日付プロパティを取得できる場合。
                    if (page.Properties == null || !page.Properties.TryGetValue(_env.DatePropertyName, out PropertyValue? datePropertyValue))
                    {
                        logBuilder.AppendLine($"[SprintReader] {pageName}: プロパティ '{_env.DatePropertyName}' が見つかりません。");
                        continue;
                    }

                    if (datePropertyValue is not DatePropertyValue dateProperty)
                    {
                        logBuilder.AppendLine($"[SprintReader] {pageName}: プロパティ '{_env.DatePropertyName}' の型が Date ではありません (型: {datePropertyValue?.Type.ToString() ?? "unknown"})。");
                        continue;
                    }

                    if (dateProperty.Date == null)
                    {
                        logBuilder.AppendLine($"[SprintReader] {pageName}: 日付プロパティの中身が空です。");
                        continue;
                    }

                    DateTime startDate = default;
                    DateTime endDate = default;

                    var dateInfo = dateProperty.Date;
                    if (dateInfo == null || !DateTimeUtility.ConvertDateUtcToJst(dateInfo.Start?.UtcDateTime, out startDate))
                    {
                        logBuilder.AppendLine($"[SprintReader] {pageName}: 開始日時が設定されていないか、変換に失敗したためスキップします。");
                        continue;
                    }

                    if (!DateTimeUtility.ConvertDateUtcToJst(dateInfo.End?.UtcDateTime, out endDate))
                    {
                        logBuilder.AppendLine($"[SprintReader] {pageName}: 終了日時が設定されていないか、変換に失敗したためスキップします。");
                        continue;
                    }

                    // 時刻を無視して日付のみで比較
                    bool isInside = startDate.Date <= today && today <= endDate.Date;
                    logBuilder.AppendLine($"[SprintReader] {pageName}: 期間判定 [{startDate:yyyy/MM/dd} ～ {endDate:yyyy/MM/dd}] -> {(isInside ? "一致" : "範囲外")}");

                    if (!isInside)
                    {
                        continue;
                    }

                    matchCount++;
                    logBuilder.AppendLine($"[SprintReader] {pageName}: スプリント期間に合致しました。内容を取得します。");
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

                logBuilder.AppendLine($"[SprintReader] 評価終了 (評価数: {evaluatedCount}, 合致数: {matchCount})");
                return output.ToString();
            }
            catch (Exception ex)
            {
                logBuilder.AppendLine($"[SprintReader] スプリントリストの取得に失敗しました: {ex}");
                return string.Empty;
            }
            finally
            {
                Console.WriteLine(logBuilder.ToString());
            }
        }

        private readonly NotionReader _reader;
        private readonly NotionEnvironment _env;
    }
}
