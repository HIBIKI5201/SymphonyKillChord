using KillChord.Runtime.Utility.Constant;
using R3;
using System;

namespace KillChord.Runtime.Adaptor.InGame.Music
{
    /// <summary>
    ///     音楽の同期状態を表すStateクラス。
    /// </summary>
    public class MusicSyncState : IDisposable
    {
        /// <summary> 再生時間。 </summary>
        public double PlayTime { get; private set; }

        /// <summary> BPM。 </summary>
        public double Bpm { get; private set; }
        /// <summary> 現在の拍。 </summary>
        public int CurrentBeat => _currentBeat.Value;

        /// <summary>
        ///     現在の拍の変化を通知するストリーム。
        ///     毎フレーム自前で監視する代わりに、拍が変わった瞬間だけ購読したい場合に使用する。
        /// </summary>
        public ReadOnlyReactiveProperty<int> CurrentBeatRx => _currentBeat;
        /// <summary> 最寄りの拍。 </summary>
        public int NearestBeat { get; private set; }
        /// <summary> 正確な拍の値。 </summary>
        public double AccurateBeat { get; private set; }
        /// <summary> 1拍の長さ（秒）。 </summary>
        public double BeatLength { get; private set; }

        /// <summary>
        ///     BPMを設定する。
        /// </summary>
        /// <param name="bpm"> 設定するBPM。 </param>
        public void SetBpm(double bpm)
        {
            if (bpm <= 0d)
            {
                Bpm = 0;
                BeatLength = 0d;
                AccurateBeat = 0d;
                _currentBeat.Value = 0;
                NearestBeat = 0;
                return;
            }

            Bpm = bpm;
            BeatLength = MusicConstants.SECONDS_PER_MINUTE / Bpm;
        }

        /// <summary>
        ///     再生時間を更新し、拍の状態を再計算する。
        /// </summary>
        /// <param name="playTime"> 現在の再生時間。 </param>
        public void UpdatePlayTime(double playTime)
        {
            PlayTime = playTime;

            if (BeatLength <= 0d)
            {
                _currentBeat.Value = 0;
                NearestBeat = 0;
                AccurateBeat = 0d;
                return;
            }

            AccurateBeat = PlayTime / BeatLength;
            NearestBeat = (int)Math.Floor(AccurateBeat + MusicConstants.HALF_BEAT_THRESHOLD);

            // ReactivePropertyは同値を弾くため、拍が変わったフレームだけ購読者へ通知される。
            _currentBeat.Value = (int)Math.Floor(AccurateBeat);
        }

        /// <summary>
        ///     拍の通知ストリームを破棄する。
        /// </summary>
        public void Dispose()
        {
            _currentBeat.Dispose();
        }

        private readonly ReactiveProperty<int> _currentBeat = new(0);
    }
}
