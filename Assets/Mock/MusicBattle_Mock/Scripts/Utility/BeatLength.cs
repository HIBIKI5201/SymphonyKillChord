using Mock.MusicBattle.MusicSync;
using UnityEngine;

namespace Mock.MusicBattle.Utility
{
    public struct BeatLength
    {
        public BeatLength(float signature, float count)
        {
            _signature = signature;
            _count = count;
        }

        public double GetLength(double bpm)
        {
            double beatSeconds = 60d / bpm;

            double barSeconds = beatSeconds * 4d;// 1小節は4/4固定。
            double unitSeconds = barSeconds / _signature;

            return unitSeconds * _count;
        }
        public double GetLength(IMusicBuffer buffer) => GetLength(buffer.CurrentBpm);

        [SerializeField, Tooltip("拍子")]
        private float _signature;
        [SerializeField, Tooltip("数")]
        private float _count;

        public void Assert(Object context = null)
        {
            Debug.Assert(0 < _signature, "拍子パラメータが設定されていません", context);
            Debug.Assert(0 < _count, "数パラメータが設定されていません", context);
        }
    }
}
