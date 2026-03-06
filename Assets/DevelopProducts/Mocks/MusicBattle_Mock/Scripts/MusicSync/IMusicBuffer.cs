namespace Mock.MusicBattle.MusicSync
{
    /// <summary>
    ///     音楽バッファのインターフェース。
    /// </summary>
    public interface IMusicBuffer
    {
        /// <summary> 現在のBPMを取得します。 </summary>
        public double CurrentBpm { get; }
        /// <summary> 1拍ごとの長さ（秒）を取得します。 </summary>
        public double BeatLength { get; }
        /// <summary> 現在の拍数を取得します。 </summary>
        public double CurrentBeat { get; }
        /// <summary> BGMの固有拍子を取得します。 </summary>
        public double PropTimeSignature { get; }

        /// <summary>
        ///     小節タイミング情報を基いて、全体拍数を算出します。
        /// </summary>
        /// <param name="barTimingInfo">小節タイミング情報。</param>
        /// <returns>全体拍数。</returns>
        public double ConvertBarTimingInfoToBeat(BarTimingInfo barTimingInfo);
    }
}
