using KillChord.Runtime.View.OutGame.Navigation;
using KillChord.Runtime.Adaptor.OutGame.BattlePreparation;
using R3;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.Screen
{
    /// <summary>
    ///     戦闘準備画面 View。
    /// </summary>
    public class BattlePreparationScreen : ScreenViewBase
    {

        /// <summary> View を初期化します。 </summary>
        public BattlePreparationScreen(VisualElement rootElement, OutGameUIEvent outGameUIEvent)
            : base(rootElement, outGameUIEvent)
        {
            _backButton = rootElement.Q<Button>(BACK_BUTTON_NAME)
                ?? throw new ArgumentNullException(
                    $"[{nameof(BattlePreparationScreen)}] {BACK_BUTTON_NAME} が見つかりませんでした。");

            _startButton = rootElement.Q<Button>(START_BUTTON_NAME)
                ?? throw new ArgumentNullException(
                    $"[{nameof(BattlePreparationScreen)}] {START_BUTTON_NAME} が見つかりませんでした。");

            _skillBuildButton = rootElement.Q<Button>(SKILL_BUILD_BUTTON_NAME)
                ?? throw new ArgumentNullException(
                    $"[{nameof(BattlePreparationScreen)}] {SKILL_BUILD_BUTTON_NAME} が見つかりませんでした。");

            _equippedSkillStrip = rootElement.Q<VisualElement>(EQUIPPED_SKILL_STRIP_NAME)
                ?? throw new ArgumentNullException(
                    $"[{nameof(BattlePreparationScreen)}] {EQUIPPED_SKILL_STRIP_NAME} が見つかりませんでした。");

            _effectScrollView = rootElement.Q<ScrollView>(EFFECT_SCROLL_VIEW_NAME)
                ?? throw new ArgumentNullException(
                    $"[{nameof(BattlePreparationScreen)}] {EFFECT_SCROLL_VIEW_NAME} が見つかりませんでした。");

            _effectList = rootElement.Q<VisualElement>(EFFECT_LIST_NAME)
                ?? throw new ArgumentNullException(
                    $"[{nameof(BattlePreparationScreen)}] {EFFECT_LIST_NAME} が見つかりませんでした。");

            RegisterButtonCallback();
        }

        /// <summary>
        ///     装備スキル表示 ViewModel を接続します。
        /// </summary>
        /// <param name="viewModel"> 接続する ViewModel です。 </param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Bind(IBattlePreparationSkillViewModel viewModel)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException(nameof(viewModel));
            }

            Unbind();
            _viewModel = viewModel;
            _subscriptions = new CompositeDisposable();
            _viewModel.Skills
                .Subscribe(HandleSkillsChangedHandler)
                .AddTo(_subscriptions);
        }

        /// <summary>
        ///     装備スキル表示 ViewModel の接続を解除します。
        /// </summary>
        public void Unbind()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
            _viewModel = null;
        }

        /// <summary>
        ///     View が保持するリソースを解放します。
        /// </summary>
        public override void Dispose()
        {
            Unbind();
            base.Dispose();
            UnregisterButtonCallback();
        }

        /// <summary>
        ///     ボタンのコールバックを登録します。
        /// </summary>
        private void RegisterButtonCallback()
        {
            _backButton.RegisterCallback<ClickEvent>(OnBackButtonClicked);
            _startButton.RegisterCallback<ClickEvent>(OnStartButtonClicked);
            _skillBuildButton.RegisterCallback<ClickEvent>(OnSkillBuildButtonClicked);

            // 処理を ClickEvent で受けているため、決定操作もクリックとして流し込む。
            // キャンセル操作で戻れるため、フォーカス移動の対象からは外す。
            _backButton.ExcludeFromNavigation();
            _startButton.EnableSubmitAsClick();
            _skillBuildButton.EnableSubmitAsClick();
        }

        /// <summary>
        ///     ボタンのコールバックを解除します。
        /// </summary>
        private void UnregisterButtonCallback()
        {
            _backButton.UnregisterCallback<ClickEvent>(OnBackButtonClicked);
            _startButton.UnregisterCallback<ClickEvent>(OnStartButtonClicked);
            _skillBuildButton.UnregisterCallback<ClickEvent>(OnSkillBuildButtonClicked);
        }

        /// <summary>
        ///     画面を閉じるボタンがクリックされたときの処理です。
        /// </summary>
        private void OnBackButtonClicked(ClickEvent evt)
        {
            OutGameUIEvent.OnScreenClosed?.Invoke();
        }

        /// <summary>
        ///     ゲーム開始ボタンがクリックされたときの処理です。
        /// </summary>
        private void OnStartButtonClicked(ClickEvent evt)
        {
            OutGameUIEvent.OnStartGame?.Invoke();
        }

        /// <summary>
        ///     スキル編成ボタンがクリックされたときの処理です。
        /// </summary>
        /// <param name="evt"></param>
        private void OnSkillBuildButtonClicked(ClickEvent evt)
        {
            OutGameUIEvent.OnShownSkillBuildScreen?.Invoke();
        }

        /// <summary>
        ///     装備スキル一覧の更新を画面へ反映します。
        /// </summary>
        /// <param name="skills"> 装備スキル表示一覧です。 </param>
        private void HandleSkillsChangedHandler(
            IReadOnlyList<BattlePreparationSkillDTO> skills)
        {
            RebuildSkills(skills);
        }

        /// <summary>
        ///     装備スキル列と効果説明一覧を再構築します。
        /// </summary>
        /// <param name="skills"> 装備スキル表示一覧です。 </param>
        private void RebuildSkills(IReadOnlyList<BattlePreparationSkillDTO> skills)
        {
            _equippedSkillStrip.Clear();
            _effectList.Clear();

            for (int i = 0; i < skills.Count; i++)
            {
                BattlePreparationSkillDTO skill = skills[i];
                _equippedSkillStrip.Add(CreateEquippedSkillItem(skill));
                _effectList.Add(CreateEffectItem(skill));
            }

            _effectScrollView.scrollOffset = Vector2.zero;
        }

        /// <summary>
        ///     上部に表示する読み取り専用の装備スキル要素を生成します。
        /// </summary>
        /// <param name="skill"> 表示対象です。 </param>
        /// <returns> 画像と表示名を持つ装備スキル要素です。 </returns>
        private VisualElement CreateEquippedSkillItem(BattlePreparationSkillDTO skill)
        {
            VisualElement skillRoot = new();
            skillRoot.AddToClassList("battle-preparation-equipped-skill");
            if (!skill.HasSkill)
            {
                skillRoot.AddToClassList("battle-preparation-equipped-skill--empty");
            }

            VisualElement iconRoot = new();
            iconRoot.AddToClassList("battle-preparation-skill-icon");
            if (!skill.HasSkill)
            {
                iconRoot.AddToClassList("battle-preparation-skill-icon--empty");
            }

            if (skill.Icon != null)
            {
                Image icon = new()
                {
                    sprite = skill.Icon,
                    scaleMode = ScaleMode.ScaleToFit,
                    pickingMode = PickingMode.Ignore,
                };
                icon.AddToClassList("battle-preparation-skill-icon__image");
                iconRoot.Add(icon);
            }
            else
            {
                Label fallbackLabel = new(EMPTY_SLOT_SYMBOL);
                fallbackLabel.AddToClassList("battle-preparation-skill-icon__fallback");
                iconRoot.Add(fallbackLabel);
            }

            Label nameLabel = new(skill.DisplayName);
            nameLabel.AddToClassList("battle-preparation-equipped-skill__name");
            skillRoot.Add(iconRoot);
            skillRoot.Add(nameLabel);
            skillRoot.tooltip = skill.DisplayName;
            return skillRoot;
        }

        /// <summary>
        ///     効果一覧に表示する1スロット分の要素を生成します。
        /// </summary>
        /// <param name="skill"> 表示対象です。 </param>
        /// <returns> 効果表示要素です。 </returns>
        private VisualElement CreateEffectItem(BattlePreparationSkillDTO skill)
        {
            VisualElement item = new();
            item.AddToClassList("battle-preparation-skill-item");
            if (!skill.HasSkill)
            {
                item.AddToClassList("battle-preparation-skill-item--empty");
            }

            Label nameLabel = new(skill.DisplayName);
            nameLabel.AddToClassList("battle-preparation-skill-item__name");
            item.Add(nameLabel);

            Label comboLabel = new(skill.ComboLabel);
            comboLabel.AddToClassList("battle-preparation-skill-item__combo");
            item.Add(comboLabel);

            VisualElement skillTypeRow = new();
            skillTypeRow.AddToClassList("battle-preparation-skill-item__type-row");
            Label skillTypeHeading = new(SKILL_TYPE_HEADING);
            skillTypeHeading.AddToClassList("battle-preparation-skill-item__type-heading");
            Label skillTypeLabel = new(skill.SkillTypeLabel);
            skillTypeLabel.AddToClassList("battle-preparation-skill-item__type");
            skillTypeRow.Add(skillTypeHeading);
            skillTypeRow.Add(skillTypeLabel);
            item.Add(skillTypeRow);

            Label effectHeading = new(SKILL_EFFECT_HEADING);
            effectHeading.AddToClassList("battle-preparation-skill-item__effect-heading");
            item.Add(effectHeading);

            string description = skill.HasEffectDescription
                ? skill.EffectDescription
                : EMPTY_SLOT_SYMBOL;
            Label descriptionLabel = new(description);
            descriptionLabel.AddToClassList("battle-preparation-skill-item__description");
            item.Add(descriptionLabel);

            return item;
        }

        private const string BACK_BUTTON_NAME = "BackButton";
        private const string START_BUTTON_NAME = "StartButton";
        private const string SKILL_BUILD_BUTTON_NAME = "SkillBuildButton";
        private const string EQUIPPED_SKILL_STRIP_NAME = "EquippedSkillStrip";
        private const string EFFECT_SCROLL_VIEW_NAME = "EquippedSkillEffectScrollView";
        private const string EFFECT_LIST_NAME = "EquippedSkillEffectList";
        private const string EMPTY_SLOT_SYMBOL = "—";
        private const string SKILL_TYPE_HEADING = "スキルの種類　：";
        private const string SKILL_EFFECT_HEADING = "スキル効果";

        /// <inheritdoc />
        protected override VisualElement InitialFocusElement => _startButton;

        /// <inheritdoc />
        protected override VisualElement CancelTargetElement => _backButton;

        private readonly Button _backButton;
        private readonly Button _startButton;
        private readonly Button _skillBuildButton;
        private readonly VisualElement _equippedSkillStrip;
        private readonly ScrollView _effectScrollView;
        private readonly VisualElement _effectList;

        private IBattlePreparationSkillViewModel _viewModel;
        private CompositeDisposable _subscriptions;
    }
}
