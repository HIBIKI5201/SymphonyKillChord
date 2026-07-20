using KillChord.Runtime.Utility.Constant;
using System;

namespace KillChord.Runtime.Adaptor.InGame.Music
{
    /// <summary>
    ///     音楽の同期状態を表すStateクラス。
    /// </summary>
    public class MusicSyncState
    {
        /// <summary> 再生時間。 </summary>
        public double PlayTime { get; private set; }

        /// <summary> BPM。 </summary>
        public double Bpm { get; private set; }
        /// <summary> 現在の拍。 </summary>
        public int CurrentBeat { get; private set; }
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
                CurrentBeat = 0;
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
                CurrentBeat = 0;
                NearestBeat = 0;
                AccurateBeat = 0d;
                return;
            }

            double elapsedSeconds = PlayTime - BeatOffsetSeconds;
            AccurateBeat = elapsedSeconds > 0d ? elapsedSeconds / BeatLength : 0d;
            CurrentBeat = (int)Math.Floor(AccurateBeat);
            NearestBeat = (int)Math.Floor(AccurateBeat + MusicConstants.HALF_BEAT_THRESHOLD);
        }
    }
}
