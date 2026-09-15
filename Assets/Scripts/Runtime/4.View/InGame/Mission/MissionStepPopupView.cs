using KillChord.Runtime.Adaptor.InGame.Mission;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KillChord.Runtime.View.InGame.Mission
{
    /// <summary>
    ///     目標ステップの説明ポップアップを表示するViewクラス。
    ///     入力を受け付けないパッシブな表示のため、スマートフォンのタッチ操作UIを妨げません。
    /// </summary>
    public class MissionStepPopupView : MonoBehaviour, IMissionStepPopupView
    {
        /// <inheritdoc />
        public void Show(Sprite image)
        {
            if (_popupImage != null)
            {
                _popupImage.sprite = image;
                _popupImage.gameObject.SetActive(image != null);
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
            }
        }

        /// <inheritdoc />
        public void Hide()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
            }
        }

        [SerializeField, Tooltip("ポップアップ全体のCanvasGroup。")]
        private CanvasGroup _canvasGroup;

        [SerializeField, Tooltip("ポップアップに表示する画像のImage。")]
        private Image _popupImage;

        private void Awake()
        {
            // 入力を受け付けないパッシブな表示のため、常にfalseに固定する。
            // (表示/非表示はalphaのみで切り替える)
            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }

            Hide();
        }
    }
}
