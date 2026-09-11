using KillChord.Runtime.Adaptor.OutGame.SkillTree;
using KillChord.Runtime.View.OutGame.Screen;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.SkillTree
{
    /// <summary>
    ///     プレイヤーステータス画面のViewクラス。
    /// </summary>
    public class PlayerStatusScreenView : ScreenViewBase, IPlayerStatusShowable, IPlayerStatusViewModel
    {
        public PlayerStatusScreenView(
            VisualElement root,
            OutGameUIEvent outGameUIEvent,
            Sprite healthIcon,
            Sprite attackIcon,
            Sprite criticalChanceIcon,
            Sprite criticalDamageIcon,
            Sprite areaAttackRangeIcon) : base(root, outGameUIEvent)
        {
            _healthPreviewLabel = root.Q<Label>(name: E_NAME_HEALTH_PREVIEW_LABEL);
            if (_healthPreviewLabel == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(PlayerStatusScreenView)}] {E_NAME_HEALTH_PREVIEW_LABEL} が見つかりませんでした。");
#endif
                throw new InvalidOperationException($"Required UI element '{E_NAME_HEALTH_PREVIEW_LABEL}' not found.");
            }
            _attackPreviewLabel = root.Q<Label>(name: E_NAME_ATTACK_PREVIEW_LABEL);
            if (_attackPreviewLabel == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(PlayerStatusScreenView)}] {E_NAME_ATTACK_PREVIEW_LABEL} が見つかりませんでした。");
#endif
                throw new InvalidOperationException($"Required UI element '{E_NAME_ATTACK_PREVIEW_LABEL}' not found.");
            }
            _criticalChancePreviewLabel = root.Q<Label>(name: E_NAME_CRITICAL_CHANCE_PREVIEW_LABEL);
            if (_criticalChancePreviewLabel == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(PlayerStatusScreenView)}] {E_NAME_CRITICAL_CHANCE_PREVIEW_LABEL} が見つかりませんでした。");
#endif
                throw new InvalidOperationException($"Required UI element '{E_NAME_CRITICAL_CHANCE_PREVIEW_LABEL}' not found.");
            }
            _criticalDamagePreviewLabel = root.Q<Label>(name: E_NAME_CRITICAL_DAMAGE_PREVIEW_LABEL);
            if (_criticalDamagePreviewLabel == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(PlayerStatusScreenView)}] {E_NAME_CRITICAL_DAMAGE_PREVIEW_LABEL} が見つかりませんでした。");
#endif
                throw new InvalidOperationException($"Required UI element '{E_NAME_CRITICAL_DAMAGE_PREVIEW_LABEL}' not found.");
            }
            _areaAttackRangePreviewLabel = root.Q<Label>(name: E_NAME_AREA_ATTACK_RANGE_PREVIEW_LABEL);
            if (_areaAttackRangePreviewLabel == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(PlayerStatusScreenView)}] {E_NAME_AREA_ATTACK_RANGE_PREVIEW_LABEL} が見つかりませんでした。");
#endif
                throw new InvalidOperationException($"Required UI element '{E_NAME_AREA_ATTACK_RANGE_PREVIEW_LABEL}' not found.");
            }

            SetIcon(root.Q<Image>(name: E_NAME_HEALTH_ICON), healthIcon);
            SetIcon(root.Q<Image>(name: E_NAME_ATTACK_ICON), attackIcon);
            SetIcon(root.Q<Image>(name: E_NAME_CRITICAL_CHANCE_ICON), criticalChanceIcon);
            SetIcon(root.Q<Image>(name: E_NAME_CRITICAL_DAMAGE_ICON), criticalDamageIcon);
            SetIcon(root.Q<Image>(name: E_NAME_AREA_ATTACK_RANGE_ICON), areaAttackRangeIcon);
        }

        /// <summary>
        ///     プレイヤーステータスのデータを反映する。
        /// </summary>
        /// <param name="dto"></param>
        public void Apply(PlayerStatusDTO dto)
        {
            _healthPreviewLabel.text = FormatWithPreview(
                dto.PlayerHealth, dto.PreviewPlayerHealth, FormatTruncated);
            _attackPreviewLabel.text = FormatWithPreview(
                dto.PlayerAttack, dto.PreviewPlayerAttack, FormatTruncated);
            _criticalChancePreviewLabel.text = FormatWithPreview(
                dto.CriticalChance, dto.PreviewCriticalChance, FormatPercentage);
            _criticalDamagePreviewLabel.text = FormatWithPreview(
                dto.CriticalDamage, dto.PreviewCriticalDamage, FormatPercentage);
            _areaAttackRangePreviewLabel.text = FormatWithPreview(
                dto.AreaAttackRangeMultiplier, dto.PreviewAreaAttackRangeMultiplier, FormatMultiplier);
        }

        /// <summary>
        ///     現在値と、選択中ノードを解放した場合の値を「現在値 → 変化後の値」形式へ変換する。
        ///     変化がない場合は矢印のみを表示し、変化後の値は表示しない。
        /// </summary>
        /// <param name="current"> 現在値。 </param>
        /// <param name="preview"> 選択中ノードを解放した場合の値。 </param>
        /// <param name="format"> 数値の表示形式を決めるフォーマッタ。 </param>
        /// <returns> 「現在値 → 」または「現在値 → 変化後の値」形式の文字列。 </returns>
        private static string FormatWithPreview(float current, float preview, Func<float, string> format)
        {
            string currentText = format(current);
            if (Mathf.Approximately(current, preview))
            {
                return $"{currentText} {PREVIEW_ARROW}";
            }

            return $"{currentText} {PREVIEW_ARROW} {format(preview)}";
        }

        /// <summary>
        ///     小数点以下を切り捨てた数値を文字列へ変換する。
        /// </summary>
        /// <param name="value"> 変換する値。 </param>
        /// <returns> 小数点以下を切り捨てた文字列。 </returns>
        private static string FormatTruncated(float value)
        {
            return Math.Floor(value).ToString();
        }

        /// <summary>
        ///     比率を小数点以下切り捨てのパーセント文字列へ変換する。
        /// </summary>
        /// <param name="value"> 0から1を基準とした比率。 </param>
        /// <returns> パーセント表記の文字列。 </returns>
        private static string FormatPercentage(float value)
        {
            return $"{Math.Floor(value * 100f)}%";
        }

        /// <summary>
        ///     倍率を小数点2桁の「倍」表記へ変換する。
        /// </summary>
        /// <param name="value"> 倍率。 </param>
        /// <returns> 「倍」表記の文字列。 </returns>
        private static string FormatMultiplier(float value)
        {
            return $"{value:0.00}倍";
        }

        /// <summary>
        ///     ステータスアイコンを一度だけ設定する。
        /// </summary>
        /// <param name="icon"> アイコン要素。 </param>
        /// <param name="sprite"> 設定するアイコン。null の場合は非表示にする。 </param>
        private static void SetIcon(Image icon, Sprite sprite)
        {
            if (icon == null)
            {
                return;
            }

            icon.sprite = sprite;
            icon.style.display = sprite == null ? DisplayStyle.None : DisplayStyle.Flex;
        }

        private const string PREVIEW_ARROW = "→";

        private Label _healthPreviewLabel;
        private Label _attackPreviewLabel;
        private Label _criticalChancePreviewLabel;
        private Label _criticalDamagePreviewLabel;
        private Label _areaAttackRangePreviewLabel;

        private const string E_NAME_HEALTH_PREVIEW_LABEL = "HealthPreviewLabel";
        private const string E_NAME_ATTACK_PREVIEW_LABEL = "AttackPreviewLabel";
        private const string E_NAME_CRITICAL_CHANCE_PREVIEW_LABEL = "CriticalChancePreviewLabel";
        private const string E_NAME_CRITICAL_DAMAGE_PREVIEW_LABEL = "CriticalDamagePreviewLabel";
        private const string E_NAME_AREA_ATTACK_RANGE_PREVIEW_LABEL = "AreaAttackRangePreviewLabel";
        private const string E_NAME_HEALTH_ICON = "HealthIcon";
        private const string E_NAME_ATTACK_ICON = "AttackIcon";
        private const string E_NAME_CRITICAL_CHANCE_ICON = "CriticalChanceIcon";
        private const string E_NAME_CRITICAL_DAMAGE_ICON = "CriticalDamageIcon";
        private const string E_NAME_AREA_ATTACK_RANGE_ICON = "AreaAttackRangeIcon";
    }
}
