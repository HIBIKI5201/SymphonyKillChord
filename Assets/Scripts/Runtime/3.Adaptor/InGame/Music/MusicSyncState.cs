using KillChord.Runtime.Utility;
using System;

namespace KillChord.Runtime.Adaptor.InGame.Music
{
    /// <summary>
    ///     音楽の同期状態を表すStateクラス。
    /// </summary>
    public class MusicSyncState
    {
        public double PlayTime { get; private set; }

        public int Bpm { get; private set; }
        public int CurrentBeat { get; private set; }
        public int NearestBeat { get; private set; }
        public double AccurateBeat { get; private set; }
        public double BeatLength { get; private set; }

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