using System;

namespace SinfoniaStudio.SinfoniaOperator
{
    /// <summary>
    ///     Notionのタスクデータベースから取得した1件分のタスク情報。
    /// </summary>
    public class NotionTaskItem
    {
        public NotionTaskItem(
            string name,
            string status,
            NotionTaskCategory category,
            bool hasDate,
            DateTime startDate,
            DateTime endDate,
            string url,
            string publicUrl)
        {
            Name = name;
            Status = status;
            Category = category;
            HasDate = hasDate;
            StartDate = startDate;
            EndDate = endDate;
            Url = url;
            PublicUrl = publicUrl;
        }

        /// <summary> タスク名。 </summary>
        public string Name { get; }

        /// <summary> ステータス名。 </summary>
        public string Status { get; }

        /// <summary> 通知区分。 </summary>
        public NotionTaskCategory Category { get; }

        /// <summary> 日付プロパティが設定されているか。 </summary>
        public bool HasDate { get; }

        /// <summary> 開始日時（JST）。 </summary>
        public DateTime StartDate { get; }

        /// <summary> 終了日時（JST）。 </summary>
        public DateTime EndDate { get; }

        /// <summary> Notionの編集用URL。 </summary>
        public string Url { get; }

        /// <summary> Notionの公開用URL。 </summary>
        public string PublicUrl { get; }
    }
}
