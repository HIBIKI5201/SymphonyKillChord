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
        private const string GLYPH_ENTRY_PREFIX = "ui.input.glyph.";

        /// <summary> その入力機器に割り当てがない操作を表す、入力アイコンの翻訳値です。 </summary>
        private const string NO_GLYPH = "-";

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

        [SerializeField, Tooltip("代替ボタンの任意の補足です。未設定なら表示しません。")]
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

                BindLabel(_leftTriggerText, "lock_on", "lock_on", "lt");
                BindLabel(_rightTriggerText, "attack", "attack", "rt");
                BindLabel(_leftShoulderText, "target_left", "target_left", "lb");
                BindLabel(_rightShoulderText, "target_right", "target_right", "rb");
                BindLabel(_leftStickText, "move", "move", "jl");
                BindLabel(_rightStickText, "look", "look", "jr");
                BindLabel(_menuText, "pause", "pause", "xmenu");
                BindLabel(_eastButtonText, "attack", "attack_alt", "xb");
                BindLabel(_southButtonText, "dodge", "dodge", "xa");
                TMP_Text note = _noteText;
                if (note != null)
                {
                    _localizedTexts.Add(new LocalizedElementText(
                        "UICommon", "ui.ingame.controller_guide.note", text =>
                        {
                            if (note != null)
                            {
                                note.text = text;
                            }
                        }, note.text));
                }
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

        /// <summary>
        ///     入力機器に応じたアイコンと、現在言語の機能名を表示します。
        ///     割り当てがない機器では、その操作がないためラベルを隠します。
        /// </summary>
        /// <param name="label"> 表示先です。 </param>
        /// <param name="entry"> 操作説明の翻訳キー末尾です。 </param>
        /// <param name="glyphEntry"> 入力アイコンの翻訳キー末尾です。 </param>
        /// <param name="fallbackSpriteName"> 翻訳を取得できない場合に使うXboxアイコン名です。 </param>
        private void BindLabel(TMP_Text label, string entry, string glyphEntry, string fallbackSpriteName)
        {
            // シーン側で非表示のために削除されたラベルは、他の説明の購読を妨げない。
            if (label == null)
            {
                return;
            }

            // 初期化失敗時もPrefabの説明を維持し、再有効化時は装飾を重ねない。
            string actionName = label.text.Substring(label.text.IndexOf('\n') + 1);
            string glyph = $"<sprite name=\"{fallbackSpriteName}\">";

            // アイコンと機能名は別々に変わるため、どちらの通知でも両方を組み立て直す。
            void Apply()
            {
                if (label == null)
                {
                    return;
                }

                label.enabled = glyph != NO_GLYPH;
                label.text = $"<size={ICON_FONT_SIZE}>{glyph}</size>\n{actionName}";
            }

            _localizedTexts.Add(new LocalizedElementText(
                "UICommon", GLYPH_ENTRY_PREFIX + glyphEntry,
                text =>
                {
                    glyph = text;
                    Apply();
                }, glyph));
            _localizedTexts.Add(new LocalizedElementText(
                "UICommon", "ui.ingame.controller_guide." + entry,
                text =>
                {
                    actionName = text;
                    Apply();
                }, actionName));
        }
    }
}
