namespace KillChord.Demo
{
    /// <summary>
    ///     体験版全体とホーム滞在ごとのタイマー状態を保持します。
    /// </summary>
    public sealed class DemoSessionState : IDemoSession
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

        /// <summary> ホームで操作できる残り秒数です。 </summary>
        public float HomeRemainingSeconds =>
            System.Math.Max(0.0f, _homeTimeLimitSeconds - HomeElapsedSeconds);

        /// <summary> 体験版全体の残り秒数です。 </summary>
        public float OverallRemainingSeconds =>
            System.Math.Max(0.0f, _overallTimeLimitSeconds - OverallElapsedSeconds);

        /// <summary>
        ///     体験版の制限時間を設定します。
        /// </summary>
        /// <param name="config"> 体験版設定です。 </param>
        public void Configure(DemoExperienceConfig config)
        {
            if (config == null)
            {
                throw new System.ArgumentNullException(nameof(config));
            }

            _homeTimeLimitSeconds = config.HomeTimeLimitSeconds;
            _overallTimeLimitSeconds = config.OverallTimeLimitSeconds;
            _isConfigured = true;
        }

        /// <summary> 冒頭シナリオ開始時、またはチュートリアル戦闘やホームからの再開時に全体タイマーを開始します。 </summary>
        public void Start()
        {
            if (!_isConfigured)
            {
                throw new System.InvalidOperationException("体験版タイマーが設定されていません。");
            }

            IsStarted = true;
        }

        /// <summary> 体験版セッションを終了してタイマーを停止します。 </summary>
        public void End()
        {
            IsStarted = false;
        }

        /// <summary> 新しい体験版セッションを開始できる初期状態へ戻します。 </summary>
        public void Reset()
        {
            IsStarted = false;
            IsHomeTimeExpired = false;
            IsOverallTimeExpired = false;
            HomeElapsedSeconds = 0.0f;
            OverallElapsedSeconds = 0.0f;
        }

        /// <summary> ホームタイマーを初期状態へ戻します。 </summary>
        public void ResetHomeTimer()
        {
            HomeElapsedSeconds = 0.0f;
            IsHomeTimeExpired = false;
        }

        /// <summary>
        ///     タイマーを進めます。
        /// </summary>
        /// <param name="unscaledDeltaTime"> TimeScaleに影響されない経過秒数です。 </param>
        /// <param name="isOutGameActive"> OutGame内にいる場合はtrueです。 </param>
        public void Tick(
            float unscaledDeltaTime,
            bool isOutGameActive)
        {
            if (!IsStarted || unscaledDeltaTime <= 0.0f)
            {
                return;
            }

            OverallElapsedSeconds += unscaledDeltaTime;
            if (isOutGameActive)
            {
                HomeElapsedSeconds += unscaledDeltaTime;
            }

            IsHomeTimeExpired |= HomeElapsedSeconds >= _homeTimeLimitSeconds;
            IsOverallTimeExpired |= OverallElapsedSeconds >= _overallTimeLimitSeconds;
        }

        private float _homeTimeLimitSeconds;
        private float _overallTimeLimitSeconds;
        private bool _isConfigured;
    }
}
