using KillChord.Runtime.View.Persistent.Localization;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.UI
{
    /// <summary>
    ///     ゲームプレイ中のコントローラー操作説明を表示するViewです。
    /// </summary>
    public sealed class InGameControllerGuideView : MonoBehaviour
    {
        private const int ICON_FONT_SIZE = 22;

        [SerializeField, Tooltip("左トリガーの操作説明です。")]
        private TMP_Text _leftTriggerText;

        [SerializeField, Tooltip("右トリガーの操作説明です。")]
        private TMP_Text _rightTriggerText;

        [SerializeField, Tooltip("左ショルダーの操作説明です。")]
        private TMP_Text _leftShoulderText;

        [SerializeField, Tooltip("右ショルダーの操作説明です。")]
        private TMP_Text _rightShoulderText;

        [SerializeField, Tooltip("左スティックの操作説明です。")]
        private TMP_Text _leftStickText;

        [SerializeField, Tooltip("右スティックの操作説明です。")]
        private TMP_Text _rightStickText;

        [SerializeField, Tooltip("Menuボタンの操作説明です。")]
        private TMP_Text _menuText;

        [SerializeField, Tooltip("右側のBボタンの操作説明です。")]
        private TMP_Text _eastButtonText;

        [SerializeField, Tooltip("下側のAボタンの操作説明です。")]
        private TMP_Text _southButtonText;

        [SerializeField, Tooltip("対象切り替え条件と代替ボタンの補足です。")]
        private TMP_Text _noteText;

        private readonly List<LocalizedElementText> _localizedTexts = new();
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

                BindLabel(_leftTriggerText, "lock_on", "lt");
                BindLabel(_rightTriggerText, "attack", "rt");
                BindLabel(_leftShoulderText, "target_left", "lb");
                BindLabel(_rightShoulderText, "target_right", "rb");
                BindLabel(_leftStickText, "move", "jl");
                BindLabel(_rightStickText, "look", "jr");
                BindLabel(_menuText, "pause", "xmenu");
                BindLabel(_eastButtonText, "attack", "xb");
                BindLabel(_southButtonText, "dodge", "xa");
                _localizedTexts.Add(new LocalizedElementText(
                    "UICommon", "ui.ingame.controller_guide.note", text => _noteText.text = text, _noteText.text));
            });
        }

        /// <summary>
        ///     非表示または破棄時にローカライズの購読と待機を解除します。
        /// </summary>
        private void OnDisable()
        {
            _subscriptionRevision++;
            foreach (LocalizedElementText localizedText in _localizedTexts)
            {
                localizedText.Dispose();
            }
            _localizedTexts.Clear();
        }

        /// <summary> Xboxの素材アイコンを残して機能名だけを現在言語へ切り替えます。 </summary>
        /// <param name="label"> 表示先です。 </param>
        /// <param name="entry"> 操作説明の翻訳キー末尾です。 </param>
        /// <param name="spriteName"> 素材のSprite Assetに登録されたXboxアイコン名です。 </param>
        private void BindLabel(TMP_Text label, string entry, string spriteName)
        {
            // 初期化失敗時もPrefabの説明を維持し、再有効化時は装飾を重ねない。
            string fallback = label.text.Substring(label.text.IndexOf('\n') + 1);
            _localizedTexts.Add(new LocalizedElementText(
                "UICommon", "ui.ingame.controller_guide." + entry,
                text => label.text = $"<size={ICON_FONT_SIZE}><sprite name=\"{spriteName}\"></size>\n{text}", fallback));
        }
    }
}
