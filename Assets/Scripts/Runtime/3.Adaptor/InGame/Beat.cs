using System;
using UnityEngine;

namespace KillChord.Runtime.Domain
{
    [Serializable]
    public readonly struct Beat
    {
        public Beat(float signature, float count)
        {
             
            _signature = Math.Max(signature, 1);
            _count = count;
        }

        public static double GetLength(Beat beat, double bpm)
        {
            double beatSeconds = 60d / bpm;

            double barSeconds = beatSeconds * 4d; // 1小節は4/4固定。
            double unitSeconds = barSeconds / beat._signature;

            return unitSeconds * beat._count;
        }

        public double Signature => _signature;
        public double Count => _count;

        [Tooltip("拍子"), Min(0)] private readonly double _signature;

        [Tooltip("数"), Min(0)] private readonly double _count;
    }
}