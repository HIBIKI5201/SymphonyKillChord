using KillChord.Runtime.Adaptor.OutGame.SkillTree;
using KillChord.Runtime.View.OutGame.Common;
using KillChord.Runtime.View.OutGame.Navigation;
using KillChord.Runtime.View.OutGame.Screen;
using KillChord.Runtime.View.Persistent.Localization;
using System;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.SkillTree
{
    /// <summary>
    ///     スキルノード解放の確認ダイアログを表示する。
    /// </summary>
    public sealed class UnlockConfirmDialogView : IDisposable
    {
        /// <summary>
        ///     確認ダイアログを初期化する。
        /// </summary>
        /// <param name="rootElement"> スキルツリー画面のルート。 </param>
        /// <param name="outGameUIEvent"> アウトゲームUIイベント。 </param>
        public UnlockConfirmDialogView(VisualElement rootElement, OutGameUIEvent outGameUIEvent)
        {
            if (rootElement == null)
            {
                throw new ArgumentNullException(nameof(rootElement));
            }

            _outGameUIEvent = outGameUIEvent ?? throw new ArgumentNullException(nameof(outGameUIEvent));
            _dialog = rootElement.Q<VisualElement>(DIALOG_NAME)
                ?? throw new InvalidOperationException($"{DIALOG_NAME} が見つかりません。");
            _pointsLabel = rootElement.Q<Label>(POINTS_LABEL_NAME)
                ?? throw new InvalidOperationException($"{POINTS_LABEL_NAME} が見つかりません。");
            VisualElement scrollView = rootElement.Q<VisualElement>(SCROLL_VIEW_NAME)
                ?? throw new InvalidOperationException($"{SCROLL_VIEW_NAME} が見つかりません。");
            if (scrollView is ScrollView unlockConfirmScrollView)
            {
                _scrollDragManipulator = new ScrollViewDragManipulator(unlockConfirmScrollView);
            }
            _statsHeader = rootElement.Q<Label>(STATS_HEADER_NAME)
                ?? throw new InvalidOperationException($"{STATS_HEADER_NAME} が見つかりません。");
            _healthRow = rootElement.Q<VisualElement>(HEALTH_ROW_NAME)
                ?? throw new InvalidOperationException($"{HEALTH_ROW_NAME} が見つかりません。");
            _healthValueLabel = rootElement.Q<Label>(HEALTH_VALUE_LABEL_NAME)
                ?? throw new InvalidOperationException($"{HEALTH_VALUE_LABEL_NAME} が見つかりません。");
            _attackRow = rootElement.Q<VisualElement>(ATTACK_ROW_NAME)
                ?? throw new InvalidOperationException($"{ATTACK_ROW_NAME} が見つかりません。");
            _attackValueLabel = rootElement.Q<Label>(ATTACK_VALUE_LABEL_NAME)
                ?? throw new InvalidOperationException($"{ATTACK_VALUE_LABEL_NAME} が見つかりません。");
            _criticalChanceRow = rootElement.Q<VisualElement>(CRITICAL_CHANCE_ROW_NAME)
                ?? throw new InvalidOperationException($"{CRITICAL_CHANCE_ROW_NAME} が見つかりません。");
            _criticalChanceValueLabel = rootElement.Q<Label>(CRITICAL_CHANCE_VALUE_LABEL_NAME)
                ?? throw new InvalidOperationException($"{CRITICAL_CHANCE_VALUE_LABEL_NAME} が見つかりません。");
            _criticalDamageRow = rootElement.Q<VisualElement>(CRITICAL_DAMAGE_ROW_NAME)
                ?? throw new InvalidOperationException($"{CRITICAL_DAMAGE_ROW_NAME} が見つかりません。");
            _criticalDamageValueLabel = rootElement.Q<Label>(CRITICAL_DAMAGE_VALUE_LABEL_NAME)
                ?? throw new InvalidOperationException($"{CRITICAL_DAMAGE_VALUE_LABEL_NAME} が見つかりません。");
            _areaAttackRangeRow = rootElement.Q<VisualElement>(AREA_ATTACK_RANGE_ROW_NAME)
                ?? throw new InvalidOperationException($"{AREA_ATTACK_RANGE_ROW_NAME} が見つかりません。");
            _areaAttackRangeValueLabel = rootElement.Q<Label>(AREA_ATTACK_RANGE_VALUE_LABEL_NAME)
                ?? throw new InvalidOperationException($"{AREA_ATTACK_RANGE_VALUE_LABEL_NAME} が見つかりません。");
            _skillSection = rootElement.Q<VisualElement>(SKILL_SECTION_NAME)
                ?? throw new InvalidOperationException($"{SKILL_SECTION_NAME} が見つかりません。");
            _skillNameList = rootElement.Q<VisualElement>(SKILL_NAME_LIST_NAME)
                ?? throw new InvalidOperationException($"{SKILL_NAME_LIST_NAME} が見つかりません。");
            _skipRow = rootElement.Q<VisualElement>(SKIP_ROW_NAME)
                ?? throw new InvalidOperationException($"{SKIP_ROW_NAME} が見つかりません。");
            _skipCheckmark = rootElement.Q<VisualElement>(SKIP_CHECKMARK_NAME)
                ?? throw new InvalidOperationException($"{SKIP_CHECKMARK_NAME} が見つかりません。");
            _confirmButton = rootElement.Q<Button>(CONFIRM_BUTTON_NAME)
                ?? throw new InvalidOperationException($"{CONFIRM_BUTTON_NAME} が見つかりません。");

            _skipRow.MakeNavigable();
            _skipRowActivation = _skipRow.RegisterActivation(HandleSkipRowActivatedHandler);
            // Button.clicked はコントローラーの決定操作(NavigationSubmitEvent)には反応しないため、
            // MakeNavigable() とあわせて RegisterActivation() でクリックと決定操作を1つの処理へ統合する。
            _confirmButton.MakeNavigable();
            _confirmButtonActivation = _confirmButton.RegisterActivation(HandleConfirmButtonClickedHandler);
            _dialog.RegisterCallback<NavigationCancelEvent>(
                HandleDialogNavigationCancelHandler, TrickleDown.TrickleDown);
            _confirmButtonLocalizedText = new LocalizedElementText(
                UI_COMMON_TABLE, "ui.skill_tree.unlock_confirm", text => _confirmButton.text = text);
            Hide();
        }

        /// <summary> コントローラーのキャンセル操作でダイアログが閉じられた時に通知する。 </summary>
        public event Action OnCancelled;

        /// <summary>
        ///     解放確認内容を表示してダイアログを開く。
        /// </summary>
        /// <param name="dto"> 解放確認ダイアログ用のDTO。 </param>
        public void Show(UnlockConfirmDTO dto)
        {
            int pointsAfter = dto.CurrentPoints - dto.Cost;
            _pointsLabel.text = $"{POINTS_LABEL_TEXT}{dto.CurrentPoints}　→　{pointsAfter}";

            bool anyStatChanged = false;
            anyStatChanged |= ApplyStatRow(_healthRow, _healthValueLabel,
                dto.PlayerHealth, dto.PreviewPlayerHealth, SkillTreeStatValueFormatter.FormatTruncated);
            anyStatChanged |= ApplyStatRow(_attackRow, _attackValueLabel,
                dto.PlayerAttack, dto.PreviewPlayerAttack, SkillTreeStatValueFormatter.FormatTruncated);
            anyStatChanged |= ApplyStatRow(_criticalChanceRow, _criticalChanceValueLabel,
                dto.CriticalChance, dto.PreviewCriticalChance, SkillTreeStatValueFormatter.FormatPercentage);
            anyStatChanged |= ApplyStatRow(_criticalDamageRow, _criticalDamageValueLabel,
                dto.CriticalDamage, dto.PreviewCriticalDamage, SkillTreeStatValueFormatter.FormatPercentage);
            anyStatChanged |= ApplyStatRow(_areaAttackRangeRow, _areaAttackRangeValueLabel,
                dto.AreaAttackRangeMultiplier, dto.PreviewAreaAttackRangeMultiplier, SkillTreeStatValueFormatter.FormatMultiplier);
            _statsHeader.style.display = anyStatChanged ? DisplayStyle.Flex : DisplayStyle.None;

            _skillSection.style.display = dto.SkillNames.Length > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            _skillNameList.Clear();
            for (int i = 0; i < dto.SkillNames.Length; i++)
            {
                Label nameLabel = new Label(dto.SkillNames[i]);
                nameLabel.AddToClassList(SKILL_NAME_ROW_CLASS);
                _skillNameList.Add(nameLabel);
            }

            _dialog.style.display = DisplayStyle.Flex;
        }

        /// <summary>
        ///     「このウィンドウを表示しない」チェックボックスがチェックされているか。
        /// </summary>
        public bool IsSkipConfirmationChecked { get; private set; }

        /// <summary>
        ///     確認ダイアログを閉じる。
        /// </summary>
        public void Hide()
        {
            _dialog.style.display = DisplayStyle.None;
        }

        /// <summary> ダイアログのルート要素。モーダルのフォーカス閉じ込めに使用する。 </summary>
        public VisualElement DialogRoot => _dialog;

        /// <summary>
        ///     登録済みイベントを解除する。
        /// </summary>
        public void Dispose()
        {
            _skipRowActivation?.Dispose();
            _confirmButtonActivation?.Dispose();
            _dialog.UnregisterCallback<NavigationCancelEvent>(
                HandleDialogNavigationCancelHandler, TrickleDown.TrickleDown);
            OnCancelled = null;
            _confirmButtonLocalizedText.Dispose();
            if (_scrollDragManipulator != null)
            {
                _scrollDragManipulator.target = null;
                _scrollDragManipulator = null;
            }
        }

        private const string DIALOG_NAME = "UnlockConfirmDialog";
        private const string POINTS_LABEL_NAME = "UnlockConfirmPointsLabel";
        private const string SCROLL_VIEW_NAME = "UnlockConfirmScrollView";
        private const string STATS_HEADER_NAME = "UnlockConfirmStatsHeader";
        private const string HEALTH_ROW_NAME = "UnlockConfirmHealthRow";
        private const string HEALTH_VALUE_LABEL_NAME = "UnlockConfirmHealthValueLabel";
        private const string ATTACK_ROW_NAME = "UnlockConfirmAttackRow";
        private const string ATTACK_VALUE_LABEL_NAME = "UnlockConfirmAttackValueLabel";
        private const string CRITICAL_CHANCE_ROW_NAME = "UnlockConfirmCriticalChanceRow";
        private const string CRITICAL_CHANCE_VALUE_LABEL_NAME = "UnlockConfirmCriticalChanceValueLabel";
        private const string CRITICAL_DAMAGE_ROW_NAME = "UnlockConfirmCriticalDamageRow";
        private const string CRITICAL_DAMAGE_VALUE_LABEL_NAME = "UnlockConfirmCriticalDamageValueLabel";
        private const string AREA_ATTACK_RANGE_ROW_NAME = "UnlockConfirmAreaAttackRangeRow";
        private const string AREA_ATTACK_RANGE_VALUE_LABEL_NAME = "UnlockConfirmAreaAttackRangeValueLabel";
        private const string SKILL_SECTION_NAME = "UnlockConfirmSkillSection";
        private const string SKILL_NAME_LIST_NAME = "UnlockConfirmSkillNameList";
        private const string SKILL_NAME_ROW_CLASS = "unlock-confirm-skill-name-row";
        private const string SKIP_ROW_NAME = "UnlockConfirmSkipRow";
        private const string SKIP_CHECKMARK_NAME = "UnlockConfirmSkipCheckmark";
        private const string CONFIRM_BUTTON_NAME = "UnlockConfirmButton";
        private const string POINTS_LABEL_TEXT = "研究P：";
        private const string UI_COMMON_TABLE = "UICommon";

        private readonly OutGameUIEvent _outGameUIEvent;
        private readonly VisualElement _dialog;
        private readonly Label _pointsLabel;
        private ScrollViewDragManipulator _scrollDragManipulator;
        private readonly Label _statsHeader;
        private readonly VisualElement _healthRow;
        private readonly Label _healthValueLabel;
        private readonly VisualElement _attackRow;
        private readonly Label _attackValueLabel;
        private readonly VisualElement _criticalChanceRow;
        private readonly Label _criticalChanceValueLabel;
        private readonly VisualElement _criticalDamageRow;
        private readonly Label _criticalDamageValueLabel;
        private readonly VisualElement _areaAttackRangeRow;
        private readonly Label _areaAttackRangeValueLabel;
        private readonly VisualElement _skillSection;
        private readonly VisualElement _skillNameList;
        private readonly VisualElement _skipRow;
        private readonly VisualElement _skipCheckmark;
        private readonly Button _confirmButton;
        private readonly LocalizedElementText _confirmButtonLocalizedText;
        private IDisposable _skipRowActivation;
        private IDisposable _confirmButtonActivation;

        /// <summary>
        ///     変化がある場合のみ行を表示し、「現在値　→　変化後の値」を設定する。
        /// </summary>
        /// <param name="row"> 対象の行要素。 </param>
        /// <param name="valueLabel"> 値を表示するラベル。 </param>
        /// <param name="current"> 現在値。 </param>
        /// <param name="preview"> 選択中ノードを解放した場合の値。 </param>
        /// <param name="format"> 数値の表示形式を決めるフォーマッタ。 </param>
        /// <returns> 値が変化した場合はtrue。 </returns>
        private static bool ApplyStatRow(
            VisualElement row, Label valueLabel, float current, float preview, Func<float, string> format)
        {
            bool changed = !UnityEngine.Mathf.Approximately(current, preview);
            row.style.display = changed ? DisplayStyle.Flex : DisplayStyle.None;
            if (changed)
            {
                valueLabel.text = $"：{format(current)}　→　{format(preview)}";
            }

            return changed;
        }

        /// <summary>
        ///     「このウィンドウを表示しない」チェックボックスの選択状態を切り替える。
        /// </summary>
        private void HandleSkipRowActivatedHandler()
        {
            IsSkipConfirmationChecked = !IsSkipConfirmationChecked;
            _skipCheckmark.style.display = IsSkipConfirmationChecked ? DisplayStyle.Flex : DisplayStyle.None;
        }

        /// <summary>
        ///     解放確定を通知する。
        /// </summary>
        private void HandleConfirmButtonClickedHandler()
        {
            _outGameUIEvent.OnSkillUnlockConfirmed?.Invoke();
        }

        /// <summary>
        ///     コントローラーのキャンセル操作をダイアログ外クリックと同じ「閉じる」動作に変換する。
        ///     画面全体のキャンセル処理(戻る)より先に処理するため、トリクルダウンで購読する。
        /// </summary>
        /// <param name="evt"> ナビゲーションキャンセルイベント。 </param>
        private void HandleDialogNavigationCancelHandler(NavigationCancelEvent evt)
        {
            if (_dialog.resolvedStyle.display == DisplayStyle.None)
            {
                return;
            }

            Hide();
            OnCancelled?.Invoke();
            evt.StopPropagation();
        }
    }
}
