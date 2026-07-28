using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.Domain.InGame.Mission.FailCondition
{
    /// <summary>
    ///     いずれかの子条件が満たされている場合にミッションが失敗となる条件グループを表すクラス。
    /// </summary>
    public class OrFailConditionGroup : IMissionFailCondition
    {
        /// <summary>
        ///     OrFailConditionGroup クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="conditions">失敗条件のリスト。</param>
        public OrFailConditionGroup(IReadOnlyList<IMissionFailCondition> conditions)
        {
            _conditions = conditions;
        }

        /// <summary>
        ///     条件の説明文を取得します。
        /// </summary>
        /// <returns>説明文。</returns>
        public string GetDescription()
        {
            return "いずれかの条件を満たす。";
        }

        /// <summary>
        ///     条件が満たされているかどうかを判定します。
        /// </summary>
        /// <param name="progress">ミッションの進行状況。</param>
        /// <returns>条件を満たしている場合は true、そうでない場合は false。</returns>
        public bool IsSatisfied(MissionProgress progress)
        {
            if (_conditions == null || _conditions.Count == 0)
            {
                Debug.LogWarning("失敗条件が設定されていません。");
                return false;
            }

            for (int i = 0; i < _conditions.Count; i++)
            {
                if (_conditions[i].IsSatisfied(progress))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary> 失敗条件のリスト。 </summary>
        private readonly IReadOnlyList<IMissionFailCondition> _conditions;
    }
}
