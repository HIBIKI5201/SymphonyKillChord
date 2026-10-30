namespace KillChord.Runtime.Application.Persistent.Savedata
{
    /// <summary>
    ///     プレイの結果をセーブデータへ書き込まない状態かを伝える契約です。
    ///     <para> ServiceLocatorに登録されている間、ステージ進捗と再開地点の保存を止めます。 </para>
    /// </summary>
    public interface ISaveSuppression
    {
        /// <summary> 保存を止めている場合はtrueです。 </summary>
        bool IsSaveSuppressed { get; }
    }
}
