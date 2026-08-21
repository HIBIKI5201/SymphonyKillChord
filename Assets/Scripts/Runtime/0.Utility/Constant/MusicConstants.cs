using UnityEngine;

namespace KillChord.Runtime.Utility.Constant
{
    public static class MusicConstants
    {
        /// <summary> 1分の秒数。 </summary>
        public const double SECONDS_PER_MINUTE = 60d;

        /// <summary> 4/4拍子の1小節の拍数。 </summary>
        public const double STANDARD_BEATS_PER_BAR = 4d;

        /// <summary> 四捨五入の閾値。 </summary>
        public const double HALF_BEAT_THRESHOLD = 0.5d;

        /// <summary> 再生速度の基準となるBPM。 </summary>
        public const double BASE_BPM = 60d;

        /// <summary>
        ///     BPMから演出の再生速度倍率を求める。
        /// </summary>
        /// <param name="bpm"> 現在のBPMです。 </param>
        /// <returns> 基準BPMに対する再生速度倍率です。 </returns>
        public static float GetPlaybackSpeed(double bpm)
        {
            if (bpm <= 0d)
            {
                return 1f;
            }

            return (float)(bpm / BASE_BPM);
        }
    }
}
