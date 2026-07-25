using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.Domain.InGame.Mission.ClearCondition
{
    /// <summary>
    ///     複数の目標を順番に達成させ、達成の度に現在の目標が更新されるクリア条件を表すクラス。
    ///     全てのミッションのクリア条件はこのクラスとして表現される(<see cref="MissionDefinition.ClearCondition"/>参照)。
    ///     単一の条件しか持たないミッションは要素数1のシーケンスとして扱う。
    ///     現在のステップIndexは<see cref="MissionProgress"/>側に保持され、本クラス自体は状態を持ちません。
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
        ///     先頭ステップの開始処理を行います。ミッション開始時に一度だけ呼び出してください。
        /// </summary>
        /// <param name="progress">ミッションの進行状況。</param>
        public void Start(MissionProgress progress)
        {
            if (_steps != null && _steps.Count > 0)
            {
                _steps[0].Begin(progress);
            }
        }

        /// <summary>
        ///     条件が満たされているかどうかを判定します。すべてのステップが達成されている場合に満たされます。
        ///     問い合わせのみを行い、状態は変更しません。ステップの進行は<see cref="TryAdvance"/>で行います。
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

            return progress.ObjectiveStepIndex >= _steps.Count;
        }

        /// <summary>
        ///     現在のステップの達成条件が満たされていれば、次のステップへ進めます。
        /// </summary>
        /// <param name="progress">ミッションの進行状況。</param>
        public void TryAdvance(MissionProgress progress)
        {
            if (_steps == null || _steps.Count == 0)
            {
                return;
            }

            int currentIndex = progress.ObjectiveStepIndex;
            if (currentIndex >= _steps.Count)
            {
                return;
            }

            if (!_steps[currentIndex].Condition.IsSatisfied(progress))
            {
                return;
            }

            progress.AdvanceObjectiveStep();
            int newIndex = progress.ObjectiveStepIndex;
            if (newIndex < _steps.Count)
            {
                _steps[newIndex].Begin(progress);
            }
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

        /// <summary>
        ///     指定した種類の達成条件を持つステップが含まれているか確認します。
        /// </summary>
        /// <typeparam name="TCondition"> 検索する達成条件の型です。 </typeparam>
        /// <returns> 指定した種類の達成条件を持つステップが存在する場合はtrueです。 </returns>
        public bool HasStepWithCondition<TCondition>() where TCondition : IMissionClearCondition
        {
            if (_steps == null)
            {
                return false;
            }

            for (int i = 0; i < _steps.Count; i++)
            {
                if (_steps[i].Condition is TCondition)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary> 実行順のステップ一覧。 </summary>
        private readonly IReadOnlyList<ObjectiveSequenceStep> _steps;
    }
}
