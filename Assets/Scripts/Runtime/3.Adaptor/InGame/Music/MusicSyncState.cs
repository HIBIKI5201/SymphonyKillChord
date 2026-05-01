using System;

namespace KillChord.Runtime.Adaptor.InGame.Music
{

    /// <summary>
    ///     音楽の同期状態を表すStateクラス。
    /// </summary>
    public class MusicSyncState
    {
        public double PlayTime { get; set; }

        public int Bpm { get; set; }
        public int CurrentBeat { get; set; }
        public int NearestBeat { get; set; }
        public double AccurateBeat { get; set; }
        public double BeatLength { get; set; }

        public void SetBpm(int bpm)
        {
            if (bpm <= 0)
            {
                Bpm = 0;
                BeatLength = 0d;
                return;
            }

            Bpm = bpm;
            BeatLength = SECOND_PER_MINUTE / Bpm;
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
            NearestBeat = (int)Math.Round(AccurateBeat + HALF_BEAT_THRESHOLD);
        }

        private const double SECOND_PER_MINUTE = 60d;
        private const double HALF_BEAT_THRESHOLD = 0.5d;
    }
}