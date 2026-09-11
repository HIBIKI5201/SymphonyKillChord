namespace KillChord.Demo
{
    /// <summary>
    ///     体験版セッションのタイマー状態を公開する読み取り専用契約です。
    /// </summary>
    public interface IDemoSession
    {
        /// <summary> タイマーが開始済みの場合はtrueです。 </summary>
        bool IsStarted { get; }

        /// <summary> ホーム制限時間を超えた場合はtrueです。 </summary>
        bool IsHomeTimeExpired { get; }

        /// <summary> 全体制限時間を超えた場合はtrueです。 </summary>
        bool IsOverallTimeExpired { get; }

        /// <summary> ホームで操作できる残り秒数です。 </summary>
        float HomeRemainingSeconds { get; }

        /// <summary> 体験版全体の残り秒数です。 </summary>
        float OverallRemainingSeconds { get; }
    }
}
