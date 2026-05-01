using System;

namespace KillChord.Runtime.Domain.Persistent.Music
{
    /// <summary>
    ///     音楽の拍（リズム）に関する情報を表す構造体。
    /// </summary>
    [Serializable]
    public readonly struct Beat
    {
        public Beat(double signature, double count)
        {
            // TODO: 後で相談。
            if (signature <= 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(signature), "拍子は正の数でなければなりません。");
            }

            if(count <= 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "拍数は正の数でなければなりません。");
            }

            _signature = Math.Max(signature, 1);
            _count = count;
        }

        public double Signature => _signature;
        public double Count => _count;

        public static double GetLength(Beat beat, double bpm)
        {
            double beatSeconds = SECONDS_PER_MINUTE / bpm;
            double barSeconds = beatSeconds * FOUR_FOUR_BEAT_COUNT;
            double unitSeconds = barSeconds / beat._signature;

            return unitSeconds * beat._count;
        }

        private const double SECONDS_PER_MINUTE = 60d;
        private const double FOUR_FOUR_BEAT_COUNT = 4d; // 1小節は4/4固定。

        private readonly double _signature;
        private readonly double _count;
    }
}