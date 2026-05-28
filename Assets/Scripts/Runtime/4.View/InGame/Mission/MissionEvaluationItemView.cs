using KillChord.Runtime.Adaptor.InGame.Mission;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KillChord.Runtime.View.InGame.Mission
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
            if (_descriptionText != null)
            {
                _descriptionText.text = viewModel.Description;
            }

            switch (viewModel.DisplayState)
            {
                case MissionEvaluationDisplayState.Failed:
                    ApplyFailed();
                    break;

                case MissionEvaluationDisplayState.Challenging:
                    ApplyChallenging();
                    break;

                case MissionEvaluationDisplayState.Succeeded:
                    ApplySucceeded();
                    break;
            }
        }

        /// <summary>
        ///     失敗表示を適用します。
        /// </summary>
        private void ApplyFailed()
        {
            if (_checkBox != null)
            {
                _checkBox.isOn = false;
                _checkBox.interactable = false;
            }

            if (_descriptionText != null)
            {
                _descriptionText.color = _failedColor;
                _descriptionText.fontStyle = FontStyles.Normal;
            }
        }

        /// <summary>
        ///     挑戦中表示を適用します。
        /// </summary>
        private void ApplyChallenging()
        {
            if (_checkBox != null)
            {
                _checkBox.isOn = false;
                _checkBox.interactable = false;
            }

            if (_descriptionText != null)
            {
                _descriptionText.color = _challengingColor;
                _descriptionText.fontStyle = FontStyles.Normal;
            }
        }

        /// <summary>
        ///     成功表示を適用します。
        /// </summary>
        private void ApplySucceeded()
        {
            if (_checkBox != null)
            {
                _checkBox.isOn = true;
                _checkBox.interactable = false;
            }

            if (_descriptionText != null)
            {
                _descriptionText.color = _succeededColor;
                _descriptionText.fontStyle = FontStyles.Strikethrough;
            }
        }

        [SerializeField, Tooltip("達成状況を示すチェックボックス。")] private Toggle _checkBox;
        [SerializeField, Tooltip("評価項目の説明文を表示するテキスト。")] private TMP_Text _descriptionText;
        [SerializeField, Tooltip("失敗時のテキストカラー。")] private Color _failedColor;
        [SerializeField, Tooltip("挑戦中のテキストカラー。")] private Color _challengingColor;
        [SerializeField, Tooltip("成功時のテキストカラー。")] private Color _succeededColor;
    }
}
