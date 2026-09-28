using KillChord.Runtime.Adaptor.InGame.Mission;
using KillChord.Runtime.View.Persistent.Localization;
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
                // Localizationの初期化前に購読するとSelectedLocaleが未設定で例外になるため、初期化完了を待つ。
                _requestedImageEntryKey = imageEntryKey;
                _isLocalizedImagePending = true;
                LocalizationInitializer.RunWhenInitialized(
                    hasSelectedLocale => HandleLocalizationInitialized(imageEntryKey, hasSelectedLocale));
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
        private string _requestedImageEntryKey;
        private bool _isLocalizedImagePending;
        private bool _isLocalizedImageApplied;

        /// <summary>
        ///     入力を受け付けない表示専用の設定にする。
        /// </summary>
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

        /// <summary>
        ///     ローカライズ画像の購読を解除する。
        /// </summary>
        private void OnDisable()
        {
            ReleaseLocalizedImage();
        }

        /// <summary>
        ///     Localizationの初期化完了後にローカライズ画像を購読します。
        /// </summary>
        /// <param name="imageEntryKey"> 購読を要求したエントリキーです。 </param>
        /// <param name="hasSelectedLocale"> Localeが設定済みかどうかです。 </param>
        private void HandleLocalizationInitialized(string imageEntryKey, bool hasSelectedLocale)
        {
            // 初期化待機中に非表示や別ステップへ切り替わった場合は購読しない。
            if (this == null || !_isLocalizedImagePending || _requestedImageEntryKey != imageEntryKey)
            {
                return;
            }

            _isLocalizedImagePending = false;

            if (!hasSelectedLocale)
            {
                // Localeを取得できない場合はフォールバック画像の表示を維持する。
                Debug.LogWarning(
                    $"[{nameof(MissionStepPopupView)}] SelectedLocaleが未設定のため、フォールバック画像を表示します。Entry={imageEntryKey}",
                    this);
                return;
            }

            _localizedPopupImage = new LocalizedSprite();
            _localizedPopupImage.SetReference(TUTORIAL_POPUP_IMAGE_TABLE, imageEntryKey);
            _localizedPopupImage.AssetChanged += HandlePopupImageChanged;
        }

        /// <summary>
        ///     ローカライズ済みのポップアップ画像を反映します。
        /// </summary>
        /// <param name="image"> 選択中ロケールに対応する画像です。 </param>
        private void HandlePopupImageChanged(Sprite image)
        {
            _isLocalizedImageApplied = image != null;
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
            _requestedImageEntryKey = null;
            _isLocalizedImagePending = false;

            if (_localizedPopupImage == null)
            {
                return;
            }

            _localizedPopupImage.AssetChanged -= HandlePopupImageChanged;
            _localizedPopupImage = null;

            // 解放済みの画像を参照し続けると表示が乱れるため、フォールバック画像へ戻す。
            if (_isLocalizedImageApplied)
            {
                _isLocalizedImageApplied = false;
                SetPopupImage(_fallbackImage);
            }
        }
    }
}
