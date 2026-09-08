namespace KillChord.Demo
{
    /// <summary>
    ///     初回ホーム到達からの体験版タイマー状態を保持します。
    /// </summary>
    public sealed class DemoSessionState
    {
        /// <summary> タイマーが開始済みの場合はtrueです。 </summary>
        public bool IsStarted { get; private set; }

        /// <summary> ホーム制限時間を超えた場合はtrueです。 </summary>
        public bool IsHomeTimeExpired { get; private set; }

        /// <summary> 全体制限時間を超えた場合はtrueです。 </summary>
        public bool IsOverallTimeExpired { get; private set; }

        /// <summary> 経過したホーム時間です。 </summary>
        public float HomeElapsedSeconds { get; private set; }

        /// <summary> 経過した全体時間です。 </summary>
        public float OverallElapsedSeconds { get; private set; }

        /// <summary> 初回ホーム到達時に両タイマーを開始します。 </summary>
        public void Start()
        {
            IsStarted = true;
        }

        /// <summary>
        ///     タイマーを進めます。
        /// </summary>
        /// <param name="unscaledDeltaTime"> TimeScaleに影響されない経過秒数です。 </param>
        /// <param name="isOutGameActive"> OutGame内にいる場合はtrueです。 </param>
        /// <param name="config"> 体験版設定です。 </param>
        public void Tick(
            float unscaledDeltaTime,
            bool isOutGameActive,
            DemoExperienceConfig config)
        {
            if (!IsStarted || config == null || unscaledDeltaTime <= 0.0f)
            {
                return;
            }

            OverallElapsedSeconds += unscaledDeltaTime;
            if (isOutGameActive)
            {
                HomeElapsedSeconds += unscaledDeltaTime;
            }

            IsHomeTimeExpired |= HomeElapsedSeconds >= config.HomeTimeLimitSeconds;
            IsOverallTimeExpired |= OverallElapsedSeconds >= config.OverallTimeLimitSeconds;
        }
    }
}
