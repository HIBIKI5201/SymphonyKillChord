using KillChord.Runtime.View.Persistent.Localization;
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
            SetMessage(STAGE_START_ENTRY, STAGE_START_FALLBACK);
        }

        /// <summary>
        ///     ステージクリアメッセージを設定します。
        /// </summary>
        public void SetClearMessage()
        {
            SetMessage(CLEAR_ENTRY, CLEAR_FALLBACK);
        }

        /// <summary>
        ///     ゲームオーバーメッセージを設定します。
        /// </summary>
        public void SetGameOverMessage()
        {
            SetMessage(GAME_OVER_ENTRY, GAME_OVER_FALLBACK);
        }

        /// <summary>
        ///     ステージクリアの結果画面を表示します。
        /// </summary>
        public void ShowClear()
        {
            Show(CLEAR_ENTRY, CLEAR_FALLBACK);
        }

        /// <summary>
        ///     ゲームオーバーの結果画面を表示します。
        /// </summary>
        public void ShowGameOver()
        {
            Show(GAME_OVER_ENTRY, GAME_OVER_FALLBACK);
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

        private const string UI_COMMON_TABLE = "UICommon";
        private const string STAGE_START_ENTRY = "ui.battle.sequence.start";
        private const string CLEAR_ENTRY = "ui.battle.sequence.clear";
        private const string GAME_OVER_ENTRY = "ui.battle.sequence.game_over";
        private const string STAGE_START_FALLBACK = "ミッションスタート！";
        private const string CLEAR_FALLBACK = "ステージクリア！";
        private const string GAME_OVER_FALLBACK = "ゲームオーバー";

        [SerializeField, Tooltip("結果画面全体のCanvasGroup")]
        private CanvasGroup _canvasGroup;

        [SerializeField, Tooltip("結果画面に表示するメッセージのText")]
        private TextMeshProUGUI _messageText;

        private LocalizedElementText _localizedMessage;

        /// <summary>
        ///     破棄時にメッセージの変更通知を解除します。
        /// </summary>
        private void OnDestroy()
        {
            _localizedMessage?.Dispose();
            _localizedMessage = null;
        }

        /// <summary>
        ///     結果画面を表示します。
        /// </summary>
        /// <param name="entry">表示するメッセージのUICommonのキー。</param>
        /// <param name="fallback">翻訳を取得できない場合のメッセージ。</param>
        private void Show(string entry, string fallback)
        {
            SetMessage(entry, fallback);

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
        /// <param name="entry">表示するメッセージのUICommonのキー。</param>
        /// <param name="fallback">翻訳を取得できない場合のメッセージ。</param>
        private void SetMessage(string entry, string fallback)
        {
            if (_messageText == null)
            {
                return;
            }

            _localizedMessage?.Dispose();
            _localizedMessage = new LocalizedElementText(
                UI_COMMON_TABLE, entry, text => _messageText.text = text, fallback);
        }
    }
}