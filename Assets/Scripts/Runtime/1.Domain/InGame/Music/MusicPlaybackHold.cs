using System;

namespace KillChord.Runtime.Domain.InGame.Music
{
    /// <summary>
    ///     ポーズ解除のカウントダウン中に、ゲーム側の音楽時間を止めておくための状態。
    ///     BGMだけを先に再生し、ゲーム側の時間がポーズした時刻へ戻るまで同期を止める。
    ///     止めている間に進んだ分は、小節単位のずれとして音楽時間から差し引く。
    /// </summary>
    public sealed class MusicPlaybackHold
    {
        /// <summary> 同期を止めている間はtrue。 </summary>
        public bool IsHolding => _isHolding;

        /// <summary> 音源の再生時間からゲーム側の時間へ換算するときに差し引く秒数。 </summary>
        public double ShiftSeconds => _shiftSeconds;

        /// <summary> 同期を止めた時点のゲーム側の再生時間。 </summary>
        public double HeldPlayTime => _heldPlayTime;

        /// <summary>
        ///     同期を止め、差し引く秒数を加える。
        /// </summary>
        /// <param name="heldPlayTime"> 同期を止めた時点のゲーム側の再生時間。 </param>
        /// <param name="shiftSeconds"> 差し引く秒数に加える量。小節の長さの整数倍を渡す。 </param>
        /// <param name="timeoutSeconds"> 実時間でこの秒数を過ぎても戻らないときは、ずれを取り消して解除する。 </param>
        public void Begin(double heldPlayTime, double shiftSeconds, double timeoutSeconds)
        {
            if (_isHolding)
            {
                throw new InvalidOperationException("すでに同期を止めています。");
            }

            if (shiftSeconds <= 0d || double.IsNaN(shiftSeconds) || double.IsInfinity(shiftSeconds))
            {
                throw new ArgumentOutOfRangeException(nameof(shiftSeconds));
            }

            if (double.IsNaN(heldPlayTime) || double.IsInfinity(heldPlayTime))
            {
                throw new ArgumentOutOfRangeException(nameof(heldPlayTime));
            }

            _isHolding = true;
            _heldPlayTime = heldPlayTime;
            _addedShiftSeconds = shiftSeconds;
            _shiftSeconds += shiftSeconds;
            _timeoutSeconds = timeoutSeconds;
            _elapsedRealSeconds = 0d;
        }

        /// <summary>
        ///     ゲーム側の時間が止めた時刻へ届いたかを調べ、届いていれば同期を再開する。
        ///     実時間で制限を過ぎたときは、加えたずれを取り消して再開する。
        /// </summary>
        /// <param name="playTime"> 現在のずれで換算したゲーム側の再生時間。 </param>
        /// <param name="elapsedRealSeconds"> 前回の確認から経過した実時間の秒数。 </param>
        /// <returns> この呼び出しで同期を再開した場合はtrue。 </returns>
        public bool TryRelease(double playTime, double elapsedRealSeconds)
        {
            if (!_isHolding)
            {
                return false;
            }

            if (playTime >= _heldPlayTime)
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
                // 音源が進まない場合に止めたままにしないため、ずれを戻して解除する。
                Cancel();
                return true;
            }

            return false;
        }

        /// <summary>
        ///     同期の停止をやめ、今回加えたずれを取り消す。
        /// </summary>
        public void Cancel()
        {
            if (!_isHolding)
            {
                return;
            }

            _shiftSeconds -= _addedShiftSeconds;
            _addedShiftSeconds = 0d;
            _isHolding = false;
        }

        /// <summary>
        ///     曲の切り替えや停止に合わせて、ずれと停止状態を初期化する。
        /// </summary>
        public void Reset()
        {
            _isHolding = false;
            _shiftSeconds = 0d;
            _addedShiftSeconds = 0d;
            _heldPlayTime = 0d;
            _elapsedRealSeconds = 0d;
        }

        /// <summary>
        ///     止めた時刻へ届くまでに通過する拍の数を求める。
        /// </summary>
        /// <param name="playTime"> 現在のずれで換算したゲーム側の再生時間。 </param>
        /// <param name="beatLength"> 1拍の長さ（秒）。 </param>
        /// <param name="beatOffsetSeconds"> 音源先頭から最初の小節頭までの秒数。 </param>
        /// <returns> 残りの拍数。止めていないときや拍を求められないときは0。 </returns>
        public int CalculateRemainingBeatCount(double playTime, double beatLength, double beatOffsetSeconds)
        {
            if (!_isHolding || beatLength <= 0d || double.IsNaN(playTime) || double.IsInfinity(playTime))
            {
                return 0;
            }

            double heldBeatIndex = Math.Floor((_heldPlayTime - beatOffsetSeconds) / beatLength);
            double currentBeatIndex = Math.Floor((playTime - beatOffsetSeconds) / beatLength);
            double remaining = heldBeatIndex - currentBeatIndex;
            return remaining > 0d ? (int)remaining : 0;
        }

        private bool _isHolding;
        private double _shiftSeconds;
        private double _addedShiftSeconds;
        private double _heldPlayTime;
        private double _timeoutSeconds;
        private double _elapsedRealSeconds;
    }
}
