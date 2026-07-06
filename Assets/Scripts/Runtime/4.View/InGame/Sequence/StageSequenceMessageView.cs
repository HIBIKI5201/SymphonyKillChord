using TMPro;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Sequence
{
    /// <summary>
    ///     ステージの結果表示やシーケンス用メッセージ表示を行うViewクラス。
    /// </summary>
    public class StageSequenceMessageView : MonoBehaviour
    {
        /// <summary>
        ///     初期化処理。結果画面を非表示にします。
        /// </summary>
        public void Initialize()
        {
            Hide();
        }

        /// <summary>
        ///     ステージ開始メッセージを設定します。
        /// </summary>
        public void SetStageStartMessage()
        {
            SetMessage(_stageStartMessage);
        }

        /// <summary>
        ///     ステージクリアメッセージを設定します。
        /// </summary>
        public void SetClearMessage()
        {
            SetMessage(_clearMessage);
        }

        /// <summary>
        ///     ゲームオーバーメッセージを設定します。
        /// </summary>
        public void SetGameOverMessage()
        {
            SetMessage(_gameOverMessage);
        }

        /// <summary>
        ///     ステージクリアの結果画面を表示します。
        /// </summary>
        public void ShowClear()
        {
            Show(_clearMessage);
        }

        /// <summary>
        ///     ゲームオーバーの結果画面を表示します。
        /// </summary>
        public void ShowGameOver()
        {
            Show(_gameOverMessage);
        }

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

        [SerializeField, Tooltip("結果画面全体のCanvasGroup")]
        private CanvasGroup _canvasGroup;

        [SerializeField, Tooltip("結果画面に表示するメッセージのText")]
        private TextMeshProUGUI _messageText;

        [SerializeField, Tooltip("ステージ開始時に表示するメッセージ")]
        private string _stageStartMessage = "Mission Start!";

        [SerializeField, Tooltip("ステージクリア時に表示するメッセージ")]
        private string _clearMessage = "Stage Clear!";

        [SerializeField, Tooltip("ゲームオーバー時に表示するメッセージ")]
        private string _gameOverMessage = "Game Over!";

        /// <summary>
        ///     結果画面を表示します。
        /// </summary>
        /// <param name="message">表示するメッセージ。</param>
        private void Show(string message)
        {
            SetMessage(message);

            if (_canvasGroup == null)
            {
                return;
            }

            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        /// <summary>
        ///     メッセージのみを変更します。
        /// </summary>
        /// <param name="message">表示するメッセージ。</param>
        private void SetMessage(string message)
        {
            if (_messageText == null)
            {
                return;
            }

            _messageText.text = message;
        }
    }
}