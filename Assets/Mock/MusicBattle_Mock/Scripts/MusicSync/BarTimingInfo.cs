using UnityEngine;

namespace Mock.MusicBattle.MusicSync
{
    /// <summary>
    ///     小節タイミング
    /// </summary>
    public readonly struct BarTimingInfo
    {
        /// <summary>小節フラグ。現在小節は0、次の小節は1</summary>
        public long BarFlg { get; }
        /// <summary>拍子スケール</summary>
        public long TimeSignature { get; }
        /// <summary>小節中の拍数</summary>
        public long TargetBeat { get; }
        public BarTimingInfo(long barFlg, long timeSignature, long targetBeat)
        {
            BarFlg = barFlg;
            TimeSignature = timeSignature;
            TargetBeat = targetBeat;
        }
    }
}
