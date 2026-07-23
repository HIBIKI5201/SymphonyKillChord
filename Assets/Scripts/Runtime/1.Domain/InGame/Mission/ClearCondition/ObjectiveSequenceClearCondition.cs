using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.Domain.InGame.Mission.ClearCondition
{
    /// <summary>
    ///     複数の目標を順番に達成させ、達成の度に現在の目標が更新されるクリア条件を表すクラス。
    ///     チュートリアルの練習ステップやボス戦のフェーズ管理など、ミッション全体で再利用できる汎用の複合条件。
    /// </summary>
    public class ObjectiveSequenceClearCondition : IMissionClearCondition
    {
        /// <summary>
        ///     ObjectiveSequenceClearCondition クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="steps">実行順のステップ一覧。</param>
        public ObjectiveSequenceClearCondition(IReadOnlyList<ObjectiveSequenceStep> steps)
        {
            _steps = steps;
        }

        /// <summary> ステップ数を取得します。 </summary>
        public int StepCount => _steps?.Count ?? 0;

        /// <summary>
        ///     条件の説明文を取得します。
        /// </summary>
        /// <returns>説明文。</returns>
        public string GetDescription()
        {
            return "順番に目標を達成する。";
        }

        /// <summary>
        ///     条件が満たされているかどうかを判定します。すべてのステップが達成されている場合に満たされます。
        /// </summary>
        /// <param name="progress">ミッションの進行状況。</param>
        /// <returns>条件を満たしている場合は true、そうでない場合は false。</returns>
        public bool IsSatisfied(MissionProgress progress)
        {
            if (_steps == null || _steps.Count == 0)
            {
                Debug.LogWarning("目標シーケンスのステップが設定されていません。");
                return false;
            }

            for (int i = 0; i < _steps.Count; i++)
            {
                if (!_steps[i].Condition.IsSatisfied(progress))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     現在の目標のステップIndexを取得します。先頭から見て最初に未達成のステップを返します。
        ///     すべて達成済みの場合は<see cref="StepCount"/>を返します。
        /// </summary>
        /// <param name="progress">ミッションの進行状況。</param>
        /// <returns>現在のステップIndex。</returns>
        public int GetCurrentStepIndex(MissionProgress progress)
        {
            if (_steps == null)
            {
                return 0;
            }

            for (int i = 0; i < _steps.Count; i++)
            {
                if (!_steps[i].Condition.IsSatisfied(progress))
                {
                    return i;
                }
            }

            return _steps.Count;
        }

        /// <summary>
        ///     指定したIndexのステップを取得します。
        /// </summary>
        /// <param name="index">ステップIndex。</param>
        /// <returns>ステップ。範囲外の場合はnull。</returns>
        public ObjectiveSequenceStep GetStep(int index)
        {
            if (_steps == null || index < 0 || index >= _steps.Count)
            {
                return null;
            }

            return _steps[index];
        }

        /// <summary> 実行順のステップ一覧。 </summary>
        private readonly IReadOnlyList<ObjectiveSequenceStep> _steps;
    }
}
