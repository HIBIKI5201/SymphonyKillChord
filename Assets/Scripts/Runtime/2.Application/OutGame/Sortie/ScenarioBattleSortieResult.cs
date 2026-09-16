namespace KillChord.Runtime.Application.OutGame.Sortie
{
    /// <summary>
    ///     シナリオからの専用バトル出撃の終端結果です。
    /// </summary>
    public enum ScenarioBattleSortieResult
    {
        /// <summary> 遷移元終了とInGame初期化、選択整理が完了しました。 </summary>
        Started,
        /// <summary> シーン変更前に準備が失敗しました。通常帰還が必要です。 </summary>
        PreparationFailed,
        /// <summary> 別の要求が進行中、または予約候補が一致しません。 </summary>
        Busy,
        /// <summary> シーン操作開始後に失敗し、常駐側の手動復帰画面へ接続しました。 </summary>
        Failed,
    }
}
