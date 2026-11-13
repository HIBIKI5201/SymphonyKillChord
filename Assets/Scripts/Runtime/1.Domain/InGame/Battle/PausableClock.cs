namespace KillChord.Runtime.Domain.InGame.Battle
{
    /// <summary>
    ///     止めていた時間を除いて進む時計。
    ///     ポーズ中とポーズ解除のカウントダウン中に、クールダウンなどの時間を進めないために使う。
    /// </summary>
    public sealed class PausableClock
    {
        /// <summary> 時計を止めている間はtrue。 </summary>
        public bool IsStopped => _isStopped;

        /// <summary>
        ///     止めていた時間を除いた現在時刻を取得する。
        /// </summary>
        /// <param name="sourceTime"> 元になる時刻（秒）。 </param>
        /// <returns> 止めていた時間を除いた時刻（秒）。 </returns>
        public float GetTime(float sourceTime)
        {
            float currentSourceTime = _isStopped ? _stoppedAt : sourceTime;
            return currentSourceTime - _stoppedTotal;
        }

        /// <summary>
        ///     時計を止める。止めている間は時刻が進まない。
        /// </summary>
        /// <param name="sourceTime"> 止めた時点の元の時刻（秒）。 </param>
        public void Stop(float sourceTime)
        {
            if (_isStopped)
            {
                return;
            }

            _stoppedAt = sourceTime;
            _isStopped = true;
        }

        /// <summary>
        ///     時計を再び進める。止めていた時間は以後の時刻から除く。
        /// </summary>
        /// <param name="sourceTime"> 再開した時点の元の時刻（秒）。 </param>
        public void Start(float sourceTime)
        {
            if (!_isStopped)
            {
                return;
            }

            if (sourceTime > _stoppedAt)
            {
                _stoppedTotal += sourceTime - _stoppedAt;
            }

            _isStopped = false;
        }

        private bool _isStopped;
        private float _stoppedAt;
        private float _stoppedTotal;
    }
}
