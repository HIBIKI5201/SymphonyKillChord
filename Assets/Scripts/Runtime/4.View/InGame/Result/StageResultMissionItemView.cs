using KillChord.Runtime.View.Persistent.Localization;
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
                _localizedState?.Dispose();
                _localizedState = viewModel.IsCompleted
                    ? new LocalizedElementText(UI_COMMON_TABLE, "ui.result.sub_mission_achieved", ApplyStateText, "達成")
                    : new LocalizedElementText(UI_COMMON_TABLE, "ui.result.sub_mission_not_achieved", ApplyStateText, "未達成");
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

        private const string UI_COMMON_TABLE = "UICommon";

        [SerializeField, Tooltip("サブミッションの説明文を表示するText。")]
        private TMP_Text _descriptionText;

        [SerializeField, Tooltip("達成状態を表示するText。")]
        private TMP_Text _stateText;

        [SerializeField, Tooltip("達成時に表示するオブジェクト。")]
        private GameObject _completedRoot;

        [SerializeField, Tooltip("未達成時に表示するオブジェクト。")]
        private GameObject _failedRoot;

        private LocalizedElementText _localizedState;

        /// <summary>
        ///     破棄時に達成状態の文言の変更通知を解除します。
        /// </summary>
        private void OnDestroy()
        {
            _localizedState?.Dispose();
            _localizedState = null;
        }

        /// <summary>
        ///     達成状態の文言を表示します。
        /// </summary>
        /// <param name="text"> 表示する文言です。 </param>
        private void ApplyStateText(string text)
        {
            _stateText.text = text;
        }
    }
}
