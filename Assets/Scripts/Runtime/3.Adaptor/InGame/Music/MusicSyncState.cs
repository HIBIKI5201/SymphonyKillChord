using KillChord.Runtime.Utility;
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
        public int Bpm { get; private set; }
        /// <summary> 現在の拍。 </summary>
        public int CurrentBeat { get; private set; }
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
        public void SetBpm(int bpm)
        {
            if (bpm <= 0)
            {
                Bpm = 0;
                BeatLength = 0d;
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
                CurrentBeat = 0;
                NearestBeat = 0;
                AccurateBeat = 0d;
                return;
            }

            AccurateBeat = PlayTime / BeatLength;
            CurrentBeat = (int)Math.Floor(AccurateBeat);
            NearestBeat = (int)Math.Round(AccurateBeat + MusicConstants.HALF_BEAT_THRESHOLD, MidpointRounding.AwayFromZero);
        }
    }
}