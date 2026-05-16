using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Skill
{
    /// <summary>
    ///     スキル入力進行UIのRowViewとStepViewを生成するクラス。
    /// </summary>
    public class SkillInputProgressSpawner : MonoBehaviour
    {
        /// <summary>
        ///     RowViewとその中のStepViewを生成する。
        /// </summary>
        /// <param name="parent"> 親Transform。 </param>
        /// <param name="stepCount"> 生成するStepViewの数。 </param>
        /// <returns> 生成されたRowView。 </returns>
        public SkillInputProgressRowView SpawnRowWithSteps(
            Transform parent,
            int stepCount)
        {
            SkillInputProgressRowView rowView = Instantiate(_rowPrefab, parent);
            List<SkillInputProgressStepView> stepViews = new();

            for (int i = 0; i < stepCount; i++)
            {
                SkillInputProgressStepView stepView = Instantiate(_stepPrefab, rowView.StepRoot);
                stepViews.Add(stepView);
            }

            rowView.SetSteps(stepViews);
            return rowView;
        }

        [SerializeField, Tooltip("スキル1個分のRowViewプレハブ。")]
        private SkillInputProgressRowView _rowPrefab;

        [SerializeField, Tooltip("入力パターン1マス分のStepViewプレハブ。")]
        private SkillInputProgressStepView _stepPrefab;
    }
}
