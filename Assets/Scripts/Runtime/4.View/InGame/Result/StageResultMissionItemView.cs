using TMPro;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Result
{
    /// <summary>
    ///    ステージ結果のサブミッション項目を表示するViewクラス。
    /// </summary>
    public class StageResultMissionItemView : MonoBehaviour
    {
        /// <summary>
        ///     ViewModelの内容を表示へ反映します。
        /// </summary>
        public void Apply(
            StageResultMissionItemViewModel viewModel)
        {
            if (viewModel == null)
            {
                return;
            }

            if (_descriptionText != null)
            {
                _descriptionText.text = viewModel.Description;
            }

            if (_stateText != null)
            {
                _stateText.text = viewModel.IsCompleted ? _completedText : _failedText;
            }

            if (_completedRoot != null)
            {
                _completedRoot.SetActive(viewModel.IsCompleted);
            }

            if (_failedRoot != null)
            {
                _failedRoot.SetActive(!viewModel.IsCompleted);
            }
        }

        [SerializeField, Tooltip("サブミッションの説明文を表示するText。")]
        private TMP_Text _descriptionText;

        [SerializeField, Tooltip("達成状態を表示するText。")]
        private TMP_Text _stateText;

        [SerializeField, Tooltip("達成時に表示するオブジェクト。")]
        private GameObject _completedRoot;

        [SerializeField, Tooltip("未達成時に表示するオブジェクト。")]
        private GameObject _failedRoot;

        [SerializeField, Tooltip("達成時の表示文。")]
        private string _completedText = "達成";

        [SerializeField, Tooltip("未達成時の表示文。")]
        private string _failedText = "未達成";
    }
}
