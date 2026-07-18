namespace SinfoniaStudio.SinfoniaOperator
{
    /// <summary>
    ///     タスクの通知区分。
    /// </summary>
    public enum NotionTaskCategory
    {
        /// <summary> 通知対象外（期間外や日付未設定）。 </summary>
        None = 0,

        /// <summary> 本日開始のタスク。 </summary>
        Start = 1,

        /// <summary> 本日納期のタスク。 </summary>
        Deadline = 2,

        /// <summary> 納期を過ぎているタスク。 </summary>
        Overdue = 3,

        /// <summary> 完了済みのタスク。 </summary>
        Done = 4,
    }
}
