using TMPro;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Sequence
{
    /// <summary>
    ///    ステージの結果（クリア/ゲームオーバー）を表示するViewクラス。
    /// </summary>
    public class StageResultUIView : MonoBehaviour
    {
        /// <summary>
        ///    初期化処理。結果画面を非表示にします。
        /// </summary>
        public void Initialize() => Hide();

        /// <summary>
        ///     ステージクリアの結果画面を表示します。
        /// </summary>
        public void ShowClear() => Show(_clearMessage);

        /// <summary>
        ///     ゲームオーバーの結果画面を表示します。
        /// </summary>
        public void ShowGameOver() => Show(_gameOverMessage);

        /// <summary>
        ///     結果画面を非表示にします。
        /// </summary>
        public void Hide()
        {
            if (_canvasGroup == null)
            {
                return;
            }

            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        [SerializeField, Tooltip("結果画面全体ののCanvasGroup")]
        private CanvasGroup _canvasGroup;

        [SerializeField, Tooltip("結果画面に表示するメッセージのText")]
        private TextMeshProUGUI _messageText;

        [SerializeField, Tooltip("ステージクリア時に表示するメッセージ")]
        private string _clearMessage = "Stage Clear!";

        [SerializeField, Tooltip("ゲームオーバー時に表示するメッセージ")]
        private string _gameOverMessage = "Game Over!";

        /// <summary>
        ///     結果画面を表示します。
        /// </summary>
        /// <param name="message"> 表示するメッセージ。 </param>
        private void Show(string message)
        {
            if (_canvasGroup == null || _messageText == null)
            {
                return;
            }

            _messageText.text = message;
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }
    }
}
