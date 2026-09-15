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
            _panelRoot = root.Q<VisualElement>(name: E_NAME_PANEL_ROOT)
                ?? throw new InvalidOperationException($"{E_NAME_PANEL_ROOT} が見つかりませんでした。");
            _healthCurrentLabel = RequireLabel(root, E_NAME_HEALTH_CURRENT_LABEL);
            _healthArrowLabel = RequireLabel(root, E_NAME_HEALTH_ARROW_LABEL);
            _healthDeltaLabel = RequireLabel(root, E_NAME_HEALTH_DELTA_LABEL);
            _attackCurrentLabel = RequireLabel(root, E_NAME_ATTACK_CURRENT_LABEL);
            _attackArrowLabel = RequireLabel(root, E_NAME_ATTACK_ARROW_LABEL);
            _attackDeltaLabel = RequireLabel(root, E_NAME_ATTACK_DELTA_LABEL);
            _criticalChanceCurrentLabel = RequireLabel(root, E_NAME_CRITICAL_CHANCE_CURRENT_LABEL);
            _criticalChanceArrowLabel = RequireLabel(root, E_NAME_CRITICAL_CHANCE_ARROW_LABEL);
            _criticalChanceDeltaLabel = RequireLabel(root, E_NAME_CRITICAL_CHANCE_DELTA_LABEL);
            _criticalDamageCurrentLabel = RequireLabel(root, E_NAME_CRITICAL_DAMAGE_CURRENT_LABEL);
            _criticalDamageArrowLabel = RequireLabel(root, E_NAME_CRITICAL_DAMAGE_ARROW_LABEL);
            _criticalDamageDeltaLabel = RequireLabel(root, E_NAME_CRITICAL_DAMAGE_DELTA_LABEL);
            _areaAttackRangeCurrentLabel = RequireLabel(root, E_NAME_AREA_ATTACK_RANGE_CURRENT_LABEL);
            _areaAttackRangeArrowLabel = RequireLabel(root, E_NAME_AREA_ATTACK_RANGE_ARROW_LABEL);
            _areaAttackRangeDeltaLabel = RequireLabel(root, E_NAME_AREA_ATTACK_RANGE_DELTA_LABEL);

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
            bool anyChanged = false;
            anyChanged |= ApplyStat(_healthCurrentLabel, _healthArrowLabel, _healthDeltaLabel,
                dto.PlayerHealth, dto.PreviewPlayerHealth, SkillTreeStatValueFormatter.FormatTruncated);
            anyChanged |= ApplyStat(_attackCurrentLabel, _attackArrowLabel, _attackDeltaLabel,
                dto.PlayerAttack, dto.PreviewPlayerAttack, SkillTreeStatValueFormatter.FormatTruncated);
            anyChanged |= ApplyStat(_criticalChanceCurrentLabel, _criticalChanceArrowLabel, _criticalChanceDeltaLabel,
                dto.CriticalChance, dto.PreviewCriticalChance, SkillTreeStatValueFormatter.FormatPercentage);
            anyChanged |= ApplyStat(_criticalDamageCurrentLabel, _criticalDamageArrowLabel, _criticalDamageDeltaLabel,
                dto.CriticalDamage, dto.PreviewCriticalDamage, SkillTreeStatValueFormatter.FormatPercentage);
            anyChanged |= ApplyStat(_areaAttackRangeCurrentLabel, _areaAttackRangeArrowLabel, _areaAttackRangeDeltaLabel,
                dto.AreaAttackRangeMultiplier, dto.PreviewAreaAttackRangeMultiplier, SkillTreeStatValueFormatter.FormatMultiplier);

            SetDeltaColumnVisible(anyChanged);
            _panelRoot.style.width = anyChanged ? WIDE_PANEL_WIDTH : NARROW_PANEL_WIDTH;
        }

        /// <summary>
        ///     現在値ラベルへ現在値を、矢印・変化後ラベルへ変化後の値を設定する。
        ///     変化が無い場合は矢印・変化後ラベルを空文字列にする(列の幅自体は
        ///     SetDeltaColumnVisibleが行全体で揃えるため、ここでは文字列のみを制御する)。
        /// </summary>
        /// <param name="currentLabel"> 現在値を表示するラベル。 </param>
        /// <param name="arrowLabel"> 矢印を表示するラベル。 </param>
        /// <param name="deltaLabel"> 変化後の値を表示するラベル。 </param>
        /// <param name="current"> 現在値。 </param>
        /// <param name="preview"> 選択中ノードを解放した場合の値。 </param>
        /// <param name="format"> 数値の表示形式を決めるフォーマッタ。 </param>
        /// <returns> 値が変化した場合はtrue。 </returns>
        private static bool ApplyStat(
            Label currentLabel, Label arrowLabel, Label deltaLabel,
            float current, float preview, Func<float, string> format)
        {
            currentLabel.text = format(current);
            bool changed = !Mathf.Approximately(current, preview);
            arrowLabel.text = changed ? PREVIEW_ARROW : string.Empty;
            deltaLabel.text = changed ? format(preview) : string.Empty;
            return changed;
        }

        /// <summary>
        ///     全項目とも変化が無い場合は矢印・変化後の値の列自体を非表示にし、パネル幅を縮める。
        ///     変化がある項目が1つでもあれば、行ごとの差異が出ないよう全行分の列幅を揃えて表示する。
        /// </summary>
        /// <param name="visible"> 矢印・変化後の値の列を表示する場合はtrue。 </param>
        private void SetDeltaColumnVisible(bool visible)
        {
            DisplayStyle display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            _healthArrowLabel.style.display = display;
            _healthDeltaLabel.style.display = display;
            _attackArrowLabel.style.display = display;
            _attackDeltaLabel.style.display = display;
            _criticalChanceArrowLabel.style.display = display;
            _criticalChanceDeltaLabel.style.display = display;
            _criticalDamageArrowLabel.style.display = display;
            _criticalDamageDeltaLabel.style.display = display;
            _areaAttackRangeArrowLabel.style.display = display;
            _areaAttackRangeDeltaLabel.style.display = display;
        }

        /// <summary>
        ///     指定した名前のLabelを取得する。見つからない場合はエラーログを出して例外を投げる。
        /// </summary>
        /// <param name="root"> 検索対象のルート要素。 </param>
        /// <param name="name"> 取得するLabelの要素名。 </param>
        /// <returns> 取得したLabel。 </returns>
        private static Label RequireLabel(VisualElement root, string name)
        {
            Label label = root.Q<Label>(name: name);
            if (label == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[{nameof(PlayerStatusScreenView)}] {name} が見つかりませんでした。");
#endif
                throw new InvalidOperationException($"Required UI element '{name}' not found.");
            }

            return label;
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
        private const float NARROW_PANEL_WIDTH = 390f;
        private const float WIDE_PANEL_WIDTH = 520f;

        private VisualElement _panelRoot;
        private Label _healthCurrentLabel;
        private Label _healthArrowLabel;
        private Label _healthDeltaLabel;
        private Label _attackCurrentLabel;
        private Label _attackArrowLabel;
        private Label _attackDeltaLabel;
        private Label _criticalChanceCurrentLabel;
        private Label _criticalChanceArrowLabel;
        private Label _criticalChanceDeltaLabel;
        private Label _criticalDamageCurrentLabel;
        private Label _criticalDamageArrowLabel;
        private Label _criticalDamageDeltaLabel;
        private Label _areaAttackRangeCurrentLabel;
        private Label _areaAttackRangeArrowLabel;
        private Label _areaAttackRangeDeltaLabel;

        private const string E_NAME_PANEL_ROOT = "PlayerStatusPanelRoot";
        private const string E_NAME_HEALTH_CURRENT_LABEL = "HealthCurrentLabel";
        private const string E_NAME_HEALTH_ARROW_LABEL = "HealthArrowLabel";
        private const string E_NAME_HEALTH_DELTA_LABEL = "HealthDeltaLabel";
        private const string E_NAME_ATTACK_CURRENT_LABEL = "AttackCurrentLabel";
        private const string E_NAME_ATTACK_ARROW_LABEL = "AttackArrowLabel";
        private const string E_NAME_ATTACK_DELTA_LABEL = "AttackDeltaLabel";
        private const string E_NAME_CRITICAL_CHANCE_CURRENT_LABEL = "CriticalChanceCurrentLabel";
        private const string E_NAME_CRITICAL_CHANCE_ARROW_LABEL = "CriticalChanceArrowLabel";
        private const string E_NAME_CRITICAL_CHANCE_DELTA_LABEL = "CriticalChanceDeltaLabel";
        private const string E_NAME_CRITICAL_DAMAGE_CURRENT_LABEL = "CriticalDamageCurrentLabel";
        private const string E_NAME_CRITICAL_DAMAGE_ARROW_LABEL = "CriticalDamageArrowLabel";
        private const string E_NAME_CRITICAL_DAMAGE_DELTA_LABEL = "CriticalDamageDeltaLabel";
        private const string E_NAME_AREA_ATTACK_RANGE_CURRENT_LABEL = "AreaAttackRangeCurrentLabel";
        private const string E_NAME_AREA_ATTACK_RANGE_ARROW_LABEL = "AreaAttackRangeArrowLabel";
        private const string E_NAME_AREA_ATTACK_RANGE_DELTA_LABEL = "AreaAttackRangeDeltaLabel";
        private const string E_NAME_HEALTH_ICON = "HealthIcon";
        private const string E_NAME_ATTACK_ICON = "AttackIcon";
        private const string E_NAME_CRITICAL_CHANCE_ICON = "CriticalChanceIcon";
        private const string E_NAME_CRITICAL_DAMAGE_ICON = "CriticalDamageIcon";
        private const string E_NAME_AREA_ATTACK_RANGE_ICON = "AreaAttackRangeIcon";
    }
}
