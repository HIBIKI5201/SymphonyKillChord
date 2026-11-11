using UnityEngine;

namespace KillChord.Editor.Debugger.LogicGraph
{
    /// <summary>
    ///     攻撃パイプラインの1ステップ分の記録。
    /// </summary>
    public readonly struct AttackStepTraceEntry
    {
        /// <summary>
        ///     1ステップ分の記録を初期化する。
        /// </summary>
        /// <param name="stepIndex"> 0から数えたステップの位置。 </param>
        /// <param name="stepName"> ステップの型名。 </param>
        /// <param name="damageBefore"> ステップに渡したダメージ。 </param>
        /// <param name="damageAfter"> ステップが返したダメージ。 </param>
        /// <param name="criticalCountAfter"> ステップが返したクリティカル回数。 </param>
        public AttackStepTraceEntry(
            int stepIndex,
            string stepName,
            float damageBefore,
            float damageAfter,
            int criticalCountAfter)
        {
            StepIndex = stepIndex;
            StepName = stepName;
            DamageBefore = damageBefore;
            DamageAfter = damageAfter;
            CriticalCountAfter = criticalCountAfter;
        }

        /// <summary> 0から数えたステップの位置。 </summary>
        public int StepIndex { get; }

        /// <summary> ステップの型名。 </summary>
        public string StepName { get; }

        /// <summary> ステップに渡したダメージ。 </summary>
        public float DamageBefore { get; }

        /// <summary> ステップが返したダメージ。 </summary>
        public float DamageAfter { get; }

        /// <summary> ステップが返したクリティカル回数。 </summary>
        public int CriticalCountAfter { get; }

        /// <summary> このステップでダメージが変わったか。 </summary>
        public bool HasDamageChanged => !Mathf.Approximately(DamageBefore, DamageAfter);
    }
}
