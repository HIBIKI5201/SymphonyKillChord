using System.Collections.Generic;

namespace KillChord.Runtime.Adaptor.InGame.Music
{
    /// <summary>
    ///     リズムガイドの表示用データを保持するDTO。
    /// </summary>
    public readonly ref struct RhythmGuideDto
    {
        /// <summary>
        ///     新しいリズムガイドDTOを生成する。
        /// </summary>
        /// <param name="indicatorNormalized"> インジケーターの正規化位置。1が1小節でジャスト通過分だけ1を超える。 </param>
        /// <param name="currentBeatCount"> 現在の拍数。 </param>
        /// <param name="zones"> 判定ゾーンのリスト。 </param>
        /// <param name="hasTarget"> ターゲットの有無。 </param>
        /// <param name="isJustTiming"> 音楽同期サービスによる現在のジャスト成否。 </param>
        /// <param name="targetBeatCount"> チュートリアル中等のミッション指定拍数。 </param>
        /// <param name="guideLengthInBars"> タイムアウトと共有するゲージ全長の小節数。 </param>
        /// <param name="lockedBarProgress"> 攻撃硬直で入力できない範囲の長さ（小節単位）。 </param>
        public RhythmGuideDto(float indicatorNormalized, int? currentBeatCount, IReadOnlyList<RhythmGuideZoneDto> zones, bool hasTarget, bool isJustTiming, float guideLengthInBars, int? targetBeatCount = null, float lockedBarProgress = 0f)
        {
            IndicatorNormalized = indicatorNormalized;
            CurrentBeatCount = currentBeatCount;
            Zones = zones;
            HasTarget = hasTarget;
            IsJustTiming = isJustTiming;
            TargetBeatCount = targetBeatCount;
            GuideLengthInBars = guideLengthInBars;
            LockedBarProgress = lockedBarProgress;
        }

        /// <summary> インジケーターの正規化位置。1が1小節でジャスト通過分だけ1を超える。 </summary>
        public float IndicatorNormalized { get; }
        /// <summary> 現在の拍数。 </summary>
        public int? CurrentBeatCount { get; }
        /// <summary> 判定ゾーンのリスト。 </summary>
        public IReadOnlyList<RhythmGuideZoneDto> Zones { get; }
        /// <summary> ターゲットの有無。 </summary>
        public bool HasTarget { get; }
        /// <summary> 音楽同期サービスによる現在のジャスト成否。 </summary>
        public bool IsJustTiming { get; }
        /// <summary> チュートリアル等のミッション指定拍数。 </summary>
        public int? TargetBeatCount { get; }
        /// <summary> タイムアウトと共有するゲージ全長の小節数。 </summary>
        public float GuideLengthInBars { get; }
        /// <summary> 攻撃硬直で入力できない範囲の長さ（小節単位）。直前の入力位置からこの長さまでを灰色にする。 </summary>
        public float LockedBarProgress { get; }
    }
}
