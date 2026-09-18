using KillChord.Runtime.View.Persistent.Localization;
using TMPro;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.UI
{
    /// <summary>
    ///     ゲームプレイ中のコントローラー操作説明を表示するViewです。
    /// </summary>
    public sealed class InGameControllerGuideView : MonoBehaviour
    {
        [SerializeField, Tooltip("操作説明を表示するテキストです。")]
        private TMP_Text _text;

        private LocalizedElementText _localizedText;
        private int _subscriptionRevision;

        /// <summary>
        ///     ローカライズの初期化後に現在言語の操作説明を購読します。
        /// </summary>
        private void OnEnable()
        {
            int revision = ++_subscriptionRevision;
            LocalizationInitializer.RunWhenInitialized(isReady =>
            {
                // 初期化待機中の破棄や再有効化による古い購読を防ぐ。
                if (this == null || !isActiveAndEnabled || revision != _subscriptionRevision || !isReady)
                {
                    return;
                }

                _localizedText = new LocalizedElementText(
                    "UICommon", "ui.ingame.controller_guide", ApplyText, _text.text);
            });
        }

        /// <summary>
        ///     非表示または破棄時にローカライズの購読と待機を解除します。
        /// </summary>
        private void OnDisable()
        {
            _subscriptionRevision++;
            _localizedText?.Dispose();
            _localizedText = null;
        }

        /// <summary> 現在言語の操作説明を反映します。 </summary>
        /// <param name="text"> 表示する操作説明です。 </param>
        private void ApplyText(string text)
        {
            _text.text = text;
        }
    }
}
