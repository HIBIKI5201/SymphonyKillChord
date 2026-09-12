using System;
using KillChord.Runtime.Domain.InGame.Music;

namespace KillChord.Runtime.Domain.InGame.Mission.ClearCondition
{
    /// <summary>
    ///     指定した行動を一定回数以上行うとクリアとなる条件。
    /// </summary>
    public class ActionRepeatCountClearCondition : IMissionClearCondition, IObjectiveSequenceStepCondition, IObjectiveProgressReporter
    {
        /// <summary>
        ///     ActionRepeatCountClearCondition クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="actionKind">行動の種別。</param>
        /// <param name="requiredCount">必要な発動回数。</param>
        public ActionRepeatCountClearCondition(MissionActionKind actionKind, int requiredCount)
        {
            if (requiredCount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(requiredCount),
                    requiredCount,
                    "必要発動回数は1以上である必要があります。");
            }

            _actionKind = actionKind;
            _requiredCount = requiredCount;
        }

        /// <summary>
        ///     条件の説明文を取得します。
        /// </summary>
        /// <returns>説明文。</returns>
        public string GetDescription()
        {
            return $"{_actionKind}を{_requiredCount}回以上行う。";
        }

        /// <summary>
        ///     条件が満たされているかどうかを判定します。
        /// </summary>
        /// <param name="progress">ミッションの進行状況。</param>
        /// <returns>条件を満たしている場合は true、そうでない場合は false。</returns>
        public bool IsSatisfied(MissionProgress progress)
        {
            return progress.ActionRecord.GetCount(_actionKind) - _baselineCount >= _requiredCount;
        }

        /// <inheritdoc />
        public void BeginStep(MissionProgress progress)
        {
            _baselineCount = progress.ActionRecord.GetCount(_actionKind);
        }

        /// <inheritdoc />
        public int CurrentCount(MissionProgress progress)
        {
            int count = progress.ActionRecord.GetCount(_actionKind) - _baselineCount;
            return Math.Clamp(count, 0, _requiredCount);
        }

        /// <inheritdoc />
        public int RequiredCount => _requiredCount;

        /// <summary>
        ///     UI表示用の対象BeatType。ActionKindに対応するBeatTypeが存在しない場合はnull。
        ///     ActionKind自体はドメイン内部の実装詳細のため公開せず、表示に必要なBeatTypeのみを公開する。
        /// </summary>
        public BeatType? TargetBeatType => ConvertToBeatType(_actionKind);

        private readonly MissionActionKind _actionKind;
        /// <summary> 必要な発動回数。 </summary>
        private readonly int _requiredCount;
        private int _baselineCount;

        /// <summary>
        ///     MissionActionKindを対応するBeatTypeへ変換します。
        /// </summary>
        /// <param name="actionKind">変換元の行動種別。</param>
        /// <returns>対応するBeatType。対応が存在しない場合はnull。</returns>
        private static BeatType? ConvertToBeatType(MissionActionKind actionKind)
        {
            int? beatCount = actionKind switch
            {
                MissionActionKind.AttackOneBeat => 1,
                MissionActionKind.AttackTwoBeat => 2,
                MissionActionKind.AttackThreeBeat => 3,
                MissionActionKind.AttackFourBeat => 4,
                MissionActionKind.AttackSixBeat => 6,
                MissionActionKind.AttackEightBeat => 8,
                _ => (int?)null
            };

            return beatCount.HasValue ? (BeatType)beatCount.Value : null;
        }
    }
}