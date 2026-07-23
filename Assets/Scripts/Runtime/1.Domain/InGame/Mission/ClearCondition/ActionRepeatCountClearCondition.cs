using System;

namespace KillChord.Runtime.Domain.InGame.Mission.ClearCondition
{
    /// <summary>
    ///     指定した行動を一定回数以上行うとクリアとなる条件。
    /// </summary>
    public class ActionRepeatCountClearCondition : IMissionClearCondition
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
            return progress.ActionRecord.GetCount(_actionKind) >= _requiredCount;
        }

        /// <summary> 行動の種別。 </summary>
        private readonly MissionActionKind _actionKind;
        /// <summary> 必要な発動回数。 </summary>
        private readonly int _requiredCount;
    }
}
