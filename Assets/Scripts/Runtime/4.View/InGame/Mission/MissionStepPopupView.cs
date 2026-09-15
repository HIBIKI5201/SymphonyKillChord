using KillChord.Runtime.Adaptor.InGame.Mission;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace KillChord.Runtime.View.InGame.Mission
{
    /// <summary>
    ///     目標ステップの説明ポップアップを表示するViewクラス。
    ///     入力を受け付けないパッシブな表示のため、スマートフォンのタッチ操作UIを妨げません。
    /// </summary>
    public class MissionStepPopupView : MonoBehaviour, IMissionStepPopupView
    {
        private const string TUTORIAL_POPUP_IMAGE_TABLE = "TutorialPopupImages";

        /// <inheritdoc />
        public void Show(string imageEntryKey, Sprite fallbackImage)
        {
            ReleaseLocalizedImage();
            _fallbackImage = fallbackImage;
            SetPopupImage(fallbackImage);

            if (!string.IsNullOrWhiteSpace(imageEntryKey))
            {
                _localizedPopupImage = new LocalizedSprite();
                _localizedPopupImage.SetReference(TUTORIAL_POPUP_IMAGE_TABLE, imageEntryKey);
                _localizedPopupImage.AssetChanged += HandlePopupImageChanged;
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
            }
        }

        /// <inheritdoc />
        public void Hide()
        {
            ReleaseLocalizedImage();

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
            }
        }

        [SerializeField, Tooltip("ポップアップ全体のCanvasGroup。")]
        private CanvasGroup _canvasGroup;

        [SerializeField, Tooltip("ポップアップに表示する画像のImage。")]
        private Image _popupImage;

        private LocalizedSprite _localizedPopupImage;
        private Sprite _fallbackImage;

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

        private void OnDisable()
        {
            ReleaseLocalizedImage();
        }

        /// <summary>
        ///     ローカライズ済みのポップアップ画像を反映します。
        /// </summary>
        /// <param name="image"> 選択中ロケールに対応する画像です。 </param>
        private void HandlePopupImageChanged(Sprite image)
        {
            SetPopupImage(image != null ? image : _fallbackImage);
        }

        /// <summary>
        ///     ポップアップ画像を表示対象へ設定します。
        /// </summary>
        /// <param name="image"> 表示する画像です。未設定の場合は画像オブジェクトを非表示にします。 </param>
        private void SetPopupImage(Sprite image)
        {
            if (_popupImage == null)
            {
                return;
            }

            _popupImage.sprite = image;
            _popupImage.gameObject.SetActive(image != null);
        }

        /// <summary>
        ///     ローカライズ画像の変更通知を解除します。
        /// </summary>
        private void ReleaseLocalizedImage()
        {
            if (_localizedPopupImage == null)
            {
                return;
            }

            _localizedPopupImage.AssetChanged -= HandlePopupImageChanged;
            _localizedPopupImage = null;
        }
    }
}
