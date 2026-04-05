using System;

namespace KillChord.Runtime.Domain.Persistent.Music
{
    /// <summary>
    ///     音楽の拍（リズム）に関する情報を表す構造体。
    /// </summary>
    [Serializable]
    public readonly struct Beat
    {
        public Beat(float signature, float count)
        {

            _signature = Math.Max(signature, 1);
            _count = count;
        }

        public double Signature => _signature;
        public double Count => _count;

        public static double GetLength(Beat beat, double bpm)
        {
            double beatSeconds = 60d / bpm;

            double barSeconds = beatSeconds * 4d; // 1小節は4/4固定。
            double unitSeconds = barSeconds / beat._signature;

            return unitSeconds * beat._count;
        }

        private readonly double _signature;
        private readonly double _count;
    }
}