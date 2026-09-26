using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.Domain.InGame.Mission
{
    /// <summary>
    ///     子条件をまとめて判定する条件グループの共通実装。
    ///     すべての子条件を満たす（And）か、いずれかを満たす（Or）かを指定する。
    /// </summary>
    /// <typeparam name="TCondition"> 子条件の型。 </typeparam>
    public abstract class MissionConditionGroup<TCondition> where TCondition : IMissionCondition
    {
        /// <summary>
        ///     条件グループを初期化します。
        /// </summary>
        /// <param name="conditions">子条件のリスト。</param>
        /// <param name="requiresAll">すべての子条件を満たす必要がある場合は true、いずれかでよい場合は false。</param>
        /// <param name="description">条件の説明文。</param>
        /// <param name="emptyWarningMessage">子条件が無いときに出す警告。</param>
        protected MissionConditionGroup(
            IReadOnlyList<TCondition> conditions,
            bool requiresAll,
            string description,
            string emptyWarningMessage)
        {
            _conditions = conditions;
            _requiresAll = requiresAll;
            _description = description;
            _emptyWarningMessage = emptyWarningMessage;
        }

        /// <summary>
        ///     条件の説明文を取得します。
        /// </summary>
        /// <returns>説明文。</returns>
        public string GetDescription()
        {
            return _description;
        }

        /// <summary>
        ///     条件が満たされているかどうかを判定します。子条件が無い場合は満たされていない扱いにします。
        /// </summary>
        /// <param name="progress">ミッションの進行状況。</param>
        /// <returns>条件を満たしている場合は true、そうでない場合は false。</returns>
        public bool IsSatisfied(MissionProgress progress)
        {
            if (_conditions == null || _conditions.Count == 0)
            {
                Debug.LogWarning(_emptyWarningMessage);
                return false;
            }

            // And は1つでも満たさなければ false、Or は1つでも満たせば true で確定する。
            for (int i = 0; i < _conditions.Count; i++)
            {
                bool isSatisfied = _conditions[i].IsSatisfied(progress);
                if (isSatisfied != _requiresAll)
                {
                    return isSatisfied;
                }
            }

            return _requiresAll;
        }

        private readonly IReadOnlyList<TCondition> _conditions;
        private readonly bool _requiresAll;
        private readonly string _description;
        private readonly string _emptyWarningMessage;
    }
}
