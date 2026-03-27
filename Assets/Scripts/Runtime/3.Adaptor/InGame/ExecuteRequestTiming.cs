using KillChord.Runtime.Domain;
using UnityEngine;

namespace KillChord.Runtime.Adaptor
{
    public readonly struct ExecuteRequestTiming
    {
        public ExecuteRequestTiming(byte barFlag, Beat beat)
        {
            _barFlag = barFlag;
            _beat = beat;
        }

        public byte BarFlag => _barFlag;
        public Beat Beat => _beat;

        private readonly byte _barFlag;
        private readonly Beat _beat;
    }
}