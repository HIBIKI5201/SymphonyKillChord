using KillChord.Runtime.View.InGame.Mission;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KillChord.Runtime.View
{
    public class MissionEvaluationItemView : MonoBehaviour
    {
        /// <summary>
        ///     表示内容を反映する。
        /// </summary>
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

        [SerializeField] private Toggle _checkBox;
        [SerializeField] private TMP_Text _descriptionText;
    }
}
