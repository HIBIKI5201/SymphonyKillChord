using KillChord.Runtime.Utility;
using System;

namespace KillChord.Runtime.Domain.Persistent.Music
{
    /// <summary>
    ///     音楽の拍（リズム）に関する情報を表す構造体。
    /// </summary>
    [Serializable]
    public readonly struct Beat
    {
        /// <summary>
        ///     新しい拍の情報を生成する。
        /// </summary>
        /// <param name="signature"> 拍子。 </param>
        /// <param name="count"> 拍数。 </param>
        public Beat(double signature, double count)
        {
            if (signature <= 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(signature), "拍子は正の数でなければなりません。");
            }

            if (count <= 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "拍数は正の数でなければなりません。");
            }

            _signature = signature;
            _count = count;
        }

        /// <summary> 拍子。 </summary>
        public double Signature => _signature;
        /// <summary> 拍数。 </summary>
        public double Count => _count;

        /// <summary>
        ///     指定されたBPMにおける拍の長さを取得する。
        /// </summary>
        /// <param name="beat"> 拍の情報。 </param>
        /// <param name="bpm"> BPM。 </param>
        /// <returns> 拍の長さ（秒）。 </returns>
        public static double GetLength(Beat beat, double bpm)
        {
            if (bpm <= 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(bpm), "BPMは正の数でなければなりません。");
            }

            double beatSeconds = MusicConstants.SECONDS_PER_MINUTE / bpm;
            double barSeconds = beatSeconds * MusicConstants.STANDARD_BEATS_PER_BAR;
            double unitSeconds = barSeconds / beat._signature;

            return unitSeconds * beat._count;
        }

        private readonly double _signature;
        private readonly double _count;
    }
}