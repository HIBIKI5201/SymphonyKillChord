using KillChord.Runtime.Adaptor.InGame.Mission;
using KillChord.Runtime.View.Persistent.Localization;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Mission
{
    /// <summary>
    ///     チュートリアルの説明ポップアップ1枚分の表示を行うViewクラス。
    ///     <para>
    ///         ベースPrefabがテンプレートのレイアウトを持ち、ポップアップごとのPrefabバリアントが
    ///         スクリーンショットとタイトル・本文の翻訳キーを差し替えます。
    ///         閉じる案内は、閉じる入力と現在の入力機器に合わせてアイコンを切り替えます。
    ///     </para>
    /// </summary>
    public sealed class TutorialPopupView : MonoBehaviour
    {
        /// <summary>
        ///     閉じる案内を、指定した閉じる入力に合わせて切り替えます。
        /// </summary>
        /// <param name="closeInput"> ポップアップを閉じる入力です。 </param>
        public void SetCloseInput(MissionStepPopupCloseInput closeInput)
        {
            _closeInput = closeInput;
            if (_isLocalizationReady)
            {
                ApplyCloseHint();
            }
        }

        private const string POPUP_TABLE = "TutorialPopups";
        private const string UI_COMMON_TABLE = "UICommon";
        private const string GLYPH_ENTRY_PREFIX = "ui.input.glyph.";
        private const string CLOSE_ENTRY = "tutorial.popup.close";
        private const string CLOSE_TAP_ENTRY = "tutorial.popup.close_tap";

        [SerializeField, Tooltip("タイトルのテキスト。")]
        private TMP_Text _titleText;

        [SerializeField, Tooltip("本文のテキスト。")]
        private TMP_Text _bodyText;

        [SerializeField, Tooltip("閉じる案内の入力アイコンを表示するテキスト。入力アイコンのSprite Assetを設定する。")]
        private TMP_Text _closeIconText;

        [SerializeField, Tooltip("閉じる案内の文言のテキスト。")]
        private TMP_Text _closeLabelText;

        [SerializeField, Tooltip("閉じる案内全体のルート。案内を出さない場合に非表示にする。")]
        private GameObject _closeHintRoot;

        [SerializeField, Tooltip("TutorialPopupsテーブルのタイトルのエントリーキー。")]
        private string _titleEntry;

        [SerializeField, Tooltip("TutorialPopupsテーブルの本文のエントリーキー。")]
        private string _bodyEntry;

        private readonly List<LocalizedElementText> _contentTexts = new();
        private LocalizedElementText _closeIcon;
        private LocalizedElementText _closeLabel;
        private MissionStepPopupCloseInput _closeInput = MissionStepPopupCloseInput.Submit;
        private bool _isLocalizationReady;
        private bool _isDestroyed;

        /// <summary>
        ///     Localizationの初期化後に、タイトル・本文・閉じる案内の購読を開始する。
        /// </summary>
        private void Start()
        {
            LocalizationInitializer.RunWhenInitialized(HandleLocalizationInitialized);
        }

        /// <summary>
        ///     翻訳の購読を解除する。
        /// </summary>
        private void OnDestroy()
        {
            _isDestroyed = true;
            foreach (LocalizedElementText text in _contentTexts)
            {
                text.Dispose();
            }

            _contentTexts.Clear();
            _closeIcon?.Dispose();
            _closeIcon = null;
            _closeLabel?.Dispose();
            _closeLabel = null;
        }

        /// <summary>
        ///     Localizationの初期化完了後に翻訳を購読する。
        /// </summary>
        /// <param name="isReady"> Localeを取得できたかです。 </param>
        private void HandleLocalizationInitialized(bool isReady)
        {
            if (_isDestroyed || !isReady)
            {
                return;
            }

            _isLocalizationReady = true;
            BindText(_titleText, _titleEntry);
            BindText(_bodyText, _bodyEntry);
            ApplyCloseHint();
        }

        /// <summary>
        ///     テキストへ翻訳を結び付ける。
        /// </summary>
        /// <param name="target"> 表示先のテキストです。 </param>
        /// <param name="entry"> TutorialPopupsテーブルのエントリーキーです。 </param>
        private void BindText(TMP_Text target, string entry)
        {
            if (target == null || string.IsNullOrEmpty(entry))
            {
                return;
            }

            _contentTexts.Add(new LocalizedElementText(POPUP_TABLE, entry, text => target.text = text));
        }

        /// <summary>
        ///     閉じる入力に合わせて、閉じる案内のアイコンと文言を切り替える。
        /// </summary>
        private void ApplyCloseHint()
        {
            _closeIcon?.Dispose();
            _closeIcon = null;
            _closeLabel?.Dispose();
            _closeLabel = null;

            bool hasHint = _closeInput != MissionStepPopupCloseInput.None;
            if (_closeHintRoot != null)
            {
                _closeHintRoot.SetActive(hasHint);
            }

            if (!hasHint)
            {
                return;
            }

            // スマートフォンのタップは対応するアイコンが無いため、文言だけで案内する。
            string glyphKey = ToGlyphKey(_closeInput);
            if (_closeIconText != null)
            {
                _closeIconText.gameObject.SetActive(glyphKey != null);
                if (glyphKey != null)
                {
                    _closeIcon = new LocalizedElementText(
                        UI_COMMON_TABLE,
                        GLYPH_ENTRY_PREFIX + glyphKey,
                        text => _closeIconText.text = text);
                }
            }

            if (_closeLabelText != null)
            {
                string labelEntry = _closeInput == MissionStepPopupCloseInput.ScreenTap ? CLOSE_TAP_ENTRY : CLOSE_ENTRY;
                _closeLabel = new LocalizedElementText(POPUP_TABLE, labelEntry, text => _closeLabelText.text = text);
            }
        }

        /// <summary>
        ///     閉じる入力を、UICommonの入力アイコンのキー末尾へ変換する。
        /// </summary>
        /// <param name="closeInput"> 閉じる入力です。 </param>
        /// <returns> キー末尾です。アイコンを出さない場合はnullです。 </returns>
        private static string ToGlyphKey(MissionStepPopupCloseInput closeInput)
        {
            return closeInput switch
            {
                MissionStepPopupCloseInput.Submit => "submit",
                MissionStepPopupCloseInput.Attack => "attack_button",
                MissionStepPopupCloseInput.Dodge => "dodge",
                MissionStepPopupCloseInput.Move => "move",
                MissionStepPopupCloseInput.LockOn => "lock_on",
                _ => null,
            };
        }
    }
}
