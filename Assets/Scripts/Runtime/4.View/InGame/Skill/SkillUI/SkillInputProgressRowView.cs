using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Skill
{
    /// <summary>
    ///     スキル入力の進捗を表示する行のViewクラス。
    /// </summary>
    public class SkillInputProgressRowView : MonoBehaviour
    {
        /// <summary> StepViewを並べる親Transform。 </summary>
        public Transform StepRoot => _stepRoot;

        /// <summary>
        ///     生成済みStepViewを設定する。
        /// </summary>
        /// <param name="stepViews"> 設定するStepViewのリスト。 </param>
        public void SetSteps(IReadOnlyList<SkillInputProgressStepView> stepViews)
        {
            _stepViews.Clear();
            _stepViews.AddRange(stepViews);
        }

        /// <summary>
        ///     表示データを反映する。
        /// </summary>
        public void Apply(SkillInputProgressRowData data)
        {
            for (int i = 0; i < data.Steps.Count; i++)
            {
                _stepViews[i].Apply(data.Steps[i]);
            }
        }

        [SerializeField, Tooltip("StepViewを並べる親Transform。")]
        private Transform _stepRoot;

        private readonly List<SkillInputProgressStepView> _stepViews = new();
    }
}
