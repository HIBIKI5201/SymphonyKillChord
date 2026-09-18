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

        [SerializeField, Tooltip("Menu/Optionsボタンの操作説明です。")]
        private TMP_Text _menuText;

        [SerializeField, Tooltip("右側のB/○ボタンの操作説明です。")]
        private TMP_Text _eastButtonText;

        [SerializeField, Tooltip("下側のA/×ボタンの操作説明です。")]
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

                BindLabel(_leftTriggerText, "lock_on", "LT / L2", "#40516FCC");
                BindLabel(_rightTriggerText, "attack", "RT / R2", "#40516FCC");
                BindLabel(_leftShoulderText, "target_left", "LB / L1", "#40516FCC");
                BindLabel(_rightShoulderText, "dodge_target_right", "RB / R1", "#40516FCC");
                BindLabel(_leftStickText, "move", "L Stick", "#40516FCC");
                BindLabel(_rightStickText, "look", "R Stick", "#40516FCC");
                BindLabel(_menuText, "pause", "Menu / Options", "#40516FCC");
                BindLabel(_eastButtonText, "attack", "B / ○", "#943C46CC");
                BindLabel(_southButtonText, "dodge", "A / ×", "#366749CC");
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

        /// <summary> ボタン名のバッジを残して機能名だけを現在言語へ切り替えます。 </summary>
        /// <param name="label"> 表示先です。 </param>
        /// <param name="entry"> 操作説明の翻訳キー末尾です。 </param>
        /// <param name="button"> Xbox/PlayStationのボタン名です。 </param>
        /// <param name="badgeColor"> ボタン名の背景色です。 </param>
        private void BindLabel(TMP_Text label, string entry, string button, string badgeColor)
        {
            // 初期化失敗時もPrefabの説明を維持し、再有効化時は装飾を重ねない。
            string fallback = label.text.Substring(label.text.LastIndexOf('\n') + 1);
            _localizedTexts.Add(new LocalizedElementText(
                "UICommon", "ui.ingame.controller_guide." + entry,
                text => label.text = $"<mark={badgeColor}> {button} </mark>\n{text}", fallback));
        }
    }
}
