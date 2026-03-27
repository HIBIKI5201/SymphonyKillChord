using System;
using UnityEngine;

namespace KillChord.Runtime.Adaptor
{
    [Serializable]
    public struct Beat
    {
        public Beat(float signature, float count)
        {
            _signature = signature;
            _count = count;
        }

        public double GetLength(double bpm)
        {
            double beatSeconds = 60d / bpm;

            double barSeconds = beatSeconds * 4d; // 1小節は4/4固定。
            double unitSeconds = barSeconds / _signature;

            return unitSeconds * _count;
        }

        [SerializeField, Tooltip("拍子"), Min(0)]
        private float _signature;

        [SerializeField, Tooltip("数"), Min(0)] private float _count;
    }
}