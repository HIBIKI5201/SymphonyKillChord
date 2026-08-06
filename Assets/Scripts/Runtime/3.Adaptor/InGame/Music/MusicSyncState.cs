using KillChord.Runtime.Domain.InGame.Music;
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
        /// <summary> 音源先頭から最初の小節頭までの秒数。 </summary>
        public double BeatOffsetSeconds { get; private set; }

        /// <summary>
        ///     BPMを設定する。
        /// </summary>
        /// <param name="bpm"> 設定するBPM。 </param>
        public void SetBpm(double bpm)
        {
            SetRhythm(bpm, 0d);
        }

        /// <summary>
        ///     BPMと音源先頭からの拍オフセットを設定する。
        /// </summary>
        /// <param name="bpm"> 設定するBPM。 </param>
        /// <param name="beatOffsetSeconds"> 音源先頭から最初の小節頭までの秒数。 </param>
        public void SetRhythm(double bpm, double beatOffsetSeconds)
        {
            if (bpm <= 0d || double.IsNaN(bpm) || double.IsInfinity(bpm))
            {
                Bpm = 0;
                BeatLength = 0d;
                BeatOffsetSeconds = 0d;
                AccurateBeat = 0d;
                _currentBeat.Value = 0;
                NearestBeat = 0;
                return;
            }

            Bpm = bpm;
            BeatLength = MusicConstants.SECONDS_PER_MINUTE / Bpm;
            BeatOffsetSeconds = double.IsNaN(beatOffsetSeconds) || double.IsInfinity(beatOffsetSeconds)
                ? 0d
                : beatOffsetSeconds;
        }

        /// <summary>
        ///     再生時間を更新し、拍の状態を再計算する。
        /// </summary>
        /// <param name="playTime"> 現在の再生時間。 </param>
        public void UpdatePlayTime(double playTime)
        {
            PlayTime = double.IsNaN(playTime) || double.IsInfinity(playTime)
                ? 0d
                : Math.Max(0d, playTime);

            if (BeatLength <= 0d)
            {
                _currentBeat.Value = 0;
                NearestBeat = 0;
                AccurateBeat = 0d;
                return;
            }

            double elapsedSeconds = PlayTime - BeatOffsetSeconds;
            AccurateBeat = elapsedSeconds > 0d ? elapsedSeconds / BeatLength : 0d;
            NearestBeat = (int)Math.Floor(AccurateBeat + MusicConstants.HALF_BEAT_THRESHOLD);

            // ReactivePropertyは同値を弾くため、拍が変わったフレームだけ購読者へ通知される。
            _currentBeat.Value = (int)Math.Floor(AccurateBeat);
        }

        /// <summary>
        ///     指定したタイミングのターゲット拍へ向かう進捗を取得する。
        ///     区間に入る前は0で、ターゲット拍の直前に1へ近づき、跨いだ時点で0へ戻る。
        /// </summary>
        /// <param name="musicSpec"> 対象となる音楽同期タイミング。 </param>
        /// <param name="leadCount"> 0から1へ変化させる区間の長さ。拍子と同じ単位で指定する。 </param>
        /// <returns> 0〜1の値。BPM未設定の場合は0。 </returns>
        public float GetNormalizedApproach(in MusicSyncSpec musicSpec, double leadCount)
        {
            if (BeatLength <= 0d)
            {
                return 0f;
            }

            return MusicTimingCalculator.CalculateNormalizedApproach(
                AccurateBeat,
                musicSpec.TimeSignature,
                musicSpec.TargetBeat,
                leadCount);
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
