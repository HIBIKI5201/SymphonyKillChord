using KillChord.Runtime.View.InGame.Mission;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KillChord.Runtime.View
{
    /// <summary>
    ///     ミッションの評価項目を表示するビュークラス。
    /// </summary>
    public class MissionEvaluationItemView : MonoBehaviour
    {
        /// <summary>
        ///     表示内容を反映します。
        /// </summary>
        /// <param name="viewModel">評価項目のビューモデル。</param>
        public void Apply(MissionEvaluationItemViewModel viewModel)
        {
            if (_checkBox != null)
            {
                _checkBox.isOn = viewModel.IsAchieved;
                _checkBox.interactable = false;
            }

            if (_descriptionText != null)
            {
                _descriptionText.text = viewModel.Description;
            }
        }

        [SerializeField, Tooltip("達成状況を示すチェックボックス。")] private Toggle _checkBox;
        [SerializeField, Tooltip("評価項目の説明文を表示するテキスト。")] private TMP_Text _descriptionText;
    }
}
