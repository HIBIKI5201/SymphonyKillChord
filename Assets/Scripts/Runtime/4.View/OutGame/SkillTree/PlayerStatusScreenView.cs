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
            _healthCurrentLabel = RequireLabel(root, E_NAME_HEALTH_CURRENT_LABEL);
            _healthDeltaLabel = RequireLabel(root, E_NAME_HEALTH_DELTA_LABEL);
            _attackCurrentLabel = RequireLabel(root, E_NAME_ATTACK_CURRENT_LABEL);
            _attackDeltaLabel = RequireLabel(root, E_NAME_ATTACK_DELTA_LABEL);
            _criticalChanceCurrentLabel = RequireLabel(root, E_NAME_CRITICAL_CHANCE_CURRENT_LABEL);
            _criticalChanceDeltaLabel = RequireLabel(root, E_NAME_CRITICAL_CHANCE_DELTA_LABEL);
            _criticalDamageCurrentLabel = RequireLabel(root, E_NAME_CRITICAL_DAMAGE_CURRENT_LABEL);
            _criticalDamageDeltaLabel = RequireLabel(root, E_NAME_CRITICAL_DAMAGE_DELTA_LABEL);
            _areaAttackRangeCurrentLabel = RequireLabel(root, E_NAME_AREA_ATTACK_RANGE_CURRENT_LABEL);
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
            ApplyStat(_healthCurrentLabel, _healthDeltaLabel,
                dto.PlayerHealth, dto.PreviewPlayerHealth, FormatTruncated);
            ApplyStat(_attackCurrentLabel, _attackDeltaLabel,
                dto.PlayerAttack, dto.PreviewPlayerAttack, FormatTruncated);
            ApplyStat(_criticalChanceCurrentLabel, _criticalChanceDeltaLabel,
                dto.CriticalChance, dto.PreviewCriticalChance, FormatPercentage);
            ApplyStat(_criticalDamageCurrentLabel, _criticalDamageDeltaLabel,
                dto.CriticalDamage, dto.PreviewCriticalDamage, FormatPercentage);
            ApplyStat(_areaAttackRangeCurrentLabel, _areaAttackRangeDeltaLabel,
                dto.AreaAttackRangeMultiplier, dto.PreviewAreaAttackRangeMultiplier, FormatMultiplier);
        }

        /// <summary>
        ///     現在値ラベルへ現在値を、変化後ラベルへ「→ 変化後の値」を設定する。
        ///     変化が無い場合は変化後ラベルを空文字列にし、矢印ごと非表示にする。
        /// </summary>
        /// <param name="currentLabel"> 現在値を表示するラベル。 </param>
        /// <param name="deltaLabel"> 変化後の値を表示するラベル。 </param>
        /// <param name="current"> 現在値。 </param>
        /// <param name="preview"> 選択中ノードを解放した場合の値。 </param>
        /// <param name="format"> 数値の表示形式を決めるフォーマッタ。 </param>
        private static void ApplyStat(
            Label currentLabel, Label deltaLabel, float current, float preview, Func<float, string> format)
        {
            currentLabel.text = format(current);
            deltaLabel.text = Mathf.Approximately(current, preview)
                ? string.Empty
                : $"{PREVIEW_ARROW} {format(preview)}";
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

        private Label _healthCurrentLabel;
        private Label _healthDeltaLabel;
        private Label _attackCurrentLabel;
        private Label _attackDeltaLabel;
        private Label _criticalChanceCurrentLabel;
        private Label _criticalChanceDeltaLabel;
        private Label _criticalDamageCurrentLabel;
        private Label _criticalDamageDeltaLabel;
        private Label _areaAttackRangeCurrentLabel;
        private Label _areaAttackRangeDeltaLabel;

        private const string E_NAME_HEALTH_CURRENT_LABEL = "HealthCurrentLabel";
        private const string E_NAME_HEALTH_DELTA_LABEL = "HealthDeltaLabel";
        private const string E_NAME_ATTACK_CURRENT_LABEL = "AttackCurrentLabel";
        private const string E_NAME_ATTACK_DELTA_LABEL = "AttackDeltaLabel";
        private const string E_NAME_CRITICAL_CHANCE_CURRENT_LABEL = "CriticalChanceCurrentLabel";
        private const string E_NAME_CRITICAL_CHANCE_DELTA_LABEL = "CriticalChanceDeltaLabel";
        private const string E_NAME_CRITICAL_DAMAGE_CURRENT_LABEL = "CriticalDamageCurrentLabel";
        private const string E_NAME_CRITICAL_DAMAGE_DELTA_LABEL = "CriticalDamageDeltaLabel";
        private const string E_NAME_AREA_ATTACK_RANGE_CURRENT_LABEL = "AreaAttackRangeCurrentLabel";
        private const string E_NAME_AREA_ATTACK_RANGE_DELTA_LABEL = "AreaAttackRangeDeltaLabel";
        private const string E_NAME_HEALTH_ICON = "HealthIcon";
        private const string E_NAME_ATTACK_ICON = "AttackIcon";
        private const string E_NAME_CRITICAL_CHANCE_ICON = "CriticalChanceIcon";
        private const string E_NAME_CRITICAL_DAMAGE_ICON = "CriticalDamageIcon";
        private const string E_NAME_AREA_ATTACK_RANGE_ICON = "AreaAttackRangeIcon";
    }
}
