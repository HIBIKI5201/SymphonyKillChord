using System;

namespace KillChord.Runtime.Domain.InGame.Music
{
    /// <summary>
    ///     ポーズ解除のカウントダウン中に、ゲーム側の音楽時間を止めておくための状態。
    ///     BGMは再開する時刻より前から鳴らし直し、ゲーム側の時間が再開する時刻へ届くまで同期を止める。
    /// </summary>
    public sealed class MusicPlaybackHold
    {
        /// <summary> 同期を止めている間はtrue。 </summary>
        public bool IsHolding => _isHolding;

        /// <summary> ゲーム側の同期を再開する時刻（ゲーム側の音楽時間・秒）。 </summary>
        public double ResumePlayTime => _resumePlayTime;

        /// <summary>
        ///     同期を止める。
        /// </summary>
        /// <param name="resumePlayTime"> ゲーム側の同期を再開する時刻。 </param>
        /// <param name="timeoutSeconds"> 実時間でこの秒数を過ぎても届かないときは、そのまま解除する。 </param>
        public void Begin(double resumePlayTime, double timeoutSeconds)
        {
            if (_isHolding)
            {
                throw new InvalidOperationException("すでに同期を止めています。");
            }

            if (double.IsNaN(resumePlayTime) || double.IsInfinity(resumePlayTime))
            {
                throw new ArgumentOutOfRangeException(nameof(resumePlayTime));
            }

            _isHolding = true;
            _resumePlayTime = resumePlayTime;
            _timeoutSeconds = timeoutSeconds;
            _elapsedRealSeconds = 0d;
        }

        /// <summary>
        ///     ゲーム側の時間が再開する時刻へ届いたかを調べ、届いていれば同期を再開する。
        ///     実時間で制限を過ぎたときも、音源が進まない場合に止めたままにしないため再開する。
        /// </summary>
        /// <param name="playTime"> 現在のゲーム側の音楽時間。 </param>
        /// <param name="elapsedRealSeconds"> 前回の確認から経過した実時間の秒数。 </param>
        /// <returns> この呼び出しで同期を再開した場合はtrue。 </returns>
        public bool TryRelease(double playTime, double elapsedRealSeconds)
        {
            if (!_isHolding)
            {
                return false;
            }

            if (playTime >= _resumePlayTime)
            {
                _isHolding = false;
                return true;
            }

            if (elapsedRealSeconds > 0d)
            {
                _elapsedRealSeconds += elapsedRealSeconds;
            }

            if (_elapsedRealSeconds >= _timeoutSeconds)
            {
                _isHolding = false;
                return true;
            }

            return false;
        }

        /// <summary>
        ///     同期の停止を取りやめる。
        /// </summary>
        public void Cancel()
        {
            _isHolding = false;
        }

        /// <summary>
        ///     状態を初期化する。
        /// </summary>
        public void Reset()
        {
            _isHolding = false;
            _resumePlayTime = 0d;
            _timeoutSeconds = 0d;
            _elapsedRealSeconds = 0d;
        }

        /// <summary>
        ///     再開する時刻までに通過する拍の頭の数を求める。
        /// </summary>
        /// <param name="playTime"> 現在のゲーム側の音楽時間。 </param>
        /// <param name="beatLength"> 1拍の長さ（秒）。 </param>
        /// <param name="beatOffsetSeconds"> 音源先頭から最初の小節頭までの秒数。 </param>
        /// <returns> 残りの拍の数。止めていないときは0。 </returns>
        public int CalculateRemainingBeatCount(double playTime, double beatLength, double beatOffsetSeconds)
        {
            if (!_isHolding || beatLength <= 0d || double.IsNaN(playTime) || double.IsInfinity(playTime))
            {
                return 0;
            }

            double resumeBeatIndex = Math.Floor((_resumePlayTime - beatOffsetSeconds) / beatLength);
            double currentBeatIndex = Math.Floor((playTime - beatOffsetSeconds) / beatLength);
            double remaining = resumeBeatIndex - currentBeatIndex;
            return remaining > 0d ? (int)remaining : 0;
        }

        private bool _isHolding;
        private double _resumePlayTime;
        private double _timeoutSeconds;
        private double _elapsedRealSeconds;
    }
}
