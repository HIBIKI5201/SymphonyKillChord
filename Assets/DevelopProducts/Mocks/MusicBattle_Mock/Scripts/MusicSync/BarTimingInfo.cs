using UnityEngine;

namespace Mock.MusicBattle.MusicSync
{
    /// <summary>
    ///     音楽の小節タイミング情報を保持する読み取り専用構造体。
    /// </summary>
    public readonly struct BarTimingInfo
    {
        /// <summary>
        ///     <see cref="BarTimingInfo"/>構造体の新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="barFlg">小節フラグ。</param>
        /// <param name="timeSignature">拍子スケール。</param>
        /// <param name="targetBeat">小節中のターゲット拍数。</param>
        public BarTimingInfo(long barFlg, long timeSignature, long targetBeat)
        {
            _barFlg = barFlg;
            _timeSignature = timeSignature;
            _targetBeat = targetBeat;
        }

        /// <summary> 小節フラグ。現在小節は0、次の小節は1。 </summary>
        public long BarFlg => _barFlg;
        /// <summary> 拍子スケール。 </summary>
        public long TimeSignature => _timeSignature;
        /// <summary> 小節中のターゲット拍数。 </summary>
        public long TargetBeat => _targetBeat;

        private readonly long _barFlg;
        private readonly long _timeSignature;
        private readonly long _targetBeat;
    }
}

