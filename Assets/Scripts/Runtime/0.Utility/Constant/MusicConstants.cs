using UnityEngine;

namespace KillChord.Runtime.Utility
{
    public static class MusicConstants
    {
        /// <summary> 1分の秒数。 </summary>
        public const double SECONDS_PER_MINUTE = 60d;

        /// <summary> 4/4拍子の1小節の拍数。 </summary>
        public const double STANDARD_BEATS_PER_BAR = 4d;

        /// <summary> 四捨五入の閾値。 </summary>
        public const double HALF_BEAT_THRESHOLD = 0.5d;
    }
}
