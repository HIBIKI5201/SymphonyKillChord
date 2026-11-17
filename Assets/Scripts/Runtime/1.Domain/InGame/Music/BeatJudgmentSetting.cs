using KillChord.Runtime.Domain.OutGame.StageSelect;
using System;

namespace KillChord.Runtime.Domain.InGame.Music
{
    /// <summary>
    ///     拍子ごとのジャスト範囲と解放条件を表す構造体。
    ///     判定範囲そのものは、使える拍子の組み合わせから <see cref="RhythmJudgmentDefinitionFactory"/> が決める。
    /// </summary>
    public readonly struct BeatJudgmentSetting
    {
        /// <summary>
        ///     新しい設定を生成する。
        /// </summary>
        /// <param name="beatType"> 拍の種類。 </param>
        /// <param name="justStartNormalized"> ジャスト開始位置（含む）。 </param>
        /// <param name="justEndNormalized"> ジャスト終了位置（含まない）。1小節を超える値も使用する。 </param>
        /// <param name="isNarrowRange"> ジャストの付近だけを判定範囲にする拍子の場合はtrue。 </param>
        /// <param name="unlockStageId"> クリアすると拍子が解放されるステージ。値が0の場合は最初から使える。 </param>
        public BeatJudgmentSetting(BeatType beatType, float justStartNormalized, float justEndNormalized,
            bool isNarrowRange, StageId unlockStageId)
        {
            if (float.IsNaN(justStartNormalized) || float.IsInfinity(justStartNormalized) || justStartNormalized < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(justStartNormalized));
            }

            if (float.IsNaN(justEndNormalized) || float.IsInfinity(justEndNormalized) || justEndNormalized <= justStartNormalized)
            {
                throw new ArgumentOutOfRangeException(nameof(justEndNormalized));
            }

            _beatType = beatType;
            _justStartNormalized = justStartNormalized;
            _justEndNormalized = justEndNormalized;
            _isNarrowRange = isNarrowRange;
            _unlockStageId = unlockStageId;
        }

        /// <summary> 拍の種類。 </summary>
        public BeatType BeatType => _beatType;
        /// <summary> ジャスト開始位置（含む）。 </summary>
        public float JustStartNormalized => _justStartNormalized;
        /// <summary> ジャスト終了位置（含まない）。 </summary>
        public float JustEndNormalized => _justEndNormalized;
        /// <summary> ジャストの付近だけを判定範囲にする拍子の場合はtrue。 </summary>
        public bool IsNarrowRange => _isNarrowRange;
        /// <summary> クリアすると拍子が解放されるステージ。 </summary>
        public StageId UnlockStageId => _unlockStageId;
        /// <summary> 解放条件を持つ場合はtrue。 </summary>
        public bool HasUnlockCondition => _unlockStageId.Value != 0;

        private readonly BeatType _beatType;
        private readonly float _justStartNormalized;
        private readonly float _justEndNormalized;
        private readonly bool _isNarrowRange;
        private readonly StageId _unlockStageId;
    }
}
