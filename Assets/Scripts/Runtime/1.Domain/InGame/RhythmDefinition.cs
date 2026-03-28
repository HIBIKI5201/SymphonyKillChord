using System;

namespace KillChord.Runtime.Domain
{
    public readonly struct RhythmDefinition
    {
        public readonly int Bpm;
        public readonly double BeatLength;

        public RhythmDefinition(int bpm)
        {
            Bpm = bpm;
            BeatLength = 60000d / Bpm;
        }

        /// <summary>
        /// 1~8拍子の中で最も近いものを取得する
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public int GetNearestSignature(double seconds)
        {
            if (Bpm <= 0) return 4;

            double beatSeconds = 60d / Bpm;
            double barSeconds = beatSeconds * 4d;

            int nearestSignature = 1;
            double minDiff = double.MaxValue;

            for (int i = 1; i <= 8; i++)
            {
                double targetSeconds = barSeconds / i;
                double diff = Math.Abs(seconds - targetSeconds);

                if (diff < minDiff)
                {
                    minDiff = diff;
                    nearestSignature = i;
                }
            }

            return nearestSignature;
        }
    }
}