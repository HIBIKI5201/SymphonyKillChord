using KillChord.Runtime.Domain;
using UnityEngine;

namespace KillChord.Runtime.Domain
{
    public readonly struct ExecuteRequestTiming
    {
        /// <param name="barFlag">現在からの相対を指定</param>
        /// <param name="beat">絶対を指定</param>
        public ExecuteRequestTiming(byte barFlag, Beat beat)
        {
            _barFlag = barFlag;
            _beat = beat;
        }

        /// <summary> 小節 </summary>
        public byte BarFlag => _barFlag;

        /// <summary> 拍データ </summary>
        public Beat Beat => _beat;

        private readonly byte _barFlag;
        private readonly Beat _beat;
    }
}