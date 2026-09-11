using KillChord.Runtime.Adaptor.OutGame.StageSelect;
using KillChord.Runtime.View.OutGame.Navigation;
using KillChord.Runtime.View.OutGame.Screen;
using LitMotion;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.View.OutGame.StageSelect
{
    /// <summary>
    ///     ステージ詳細画面の View クラス。
    ///     IStageDetailViewModel を実装し、DTO の内容を UI に反映します。
    /// </summary>
    public sealed class StageDetailScreenView : ScreenViewBase, IStageDetailViewModel, IStageDetailScreenShowable
    {
        /// <summary>
        ///     StageDetailScreenView を初期化します。
        /// </summary>
        public StageDetailScreenView(VisualElement rootElement, OutGameUIEvent outGameUIEvent)
            : base(rootElement, outGameUIEvent)
        {
            _stageNameLabel = rootElement.Q<Label>(STAGE_NAME_LABEL)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(StageDetailScreenView)}] {STAGE_NAME_LABEL} が見つかりませんでした。");

            _flavorTextLabel = rootElement.Q<Label>(FLAVOR_TEXT_LABEL)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(StageDetailScreenView)}] {FLAVOR_TEXT_LABEL} が見つかりませんでした。");

            _rewardSkillBuildLabel = rootElement.Q<Label>(REWARD_SKILL_BUILD_LABEL)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(StageDetailScreenView)}] {REWARD_SKILL_BUILD_LABEL} が見つかりませんでした。");

            _rewardSkillUnlockLabel = rootElement.Q<Label>(REWARD_SKILL_UNLOCK_LABEL)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(StageDetailScreenView)}] {REWARD_SKILL_UNLOCK_LABEL} が見つかりませんでした。");

            _subMissionLabel1 = rootElement.Q<Label>(SUB_MISSION_LABEL1)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(StageDetailScreenView)}] {SUB_MISSION_LABEL1} が見つかりませんでした。");

            _subMissionLabel2 = rootElement.Q<Label>(SUB_MISSION_LABEL2)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(StageDetailScreenView)}] {SUB_MISSION_LABEL2} が見つかりませんでした。");

            _subMissionLabel3 = rootElement.Q<Label>(SUB_MISSION_LABEL3)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(StageDetailScreenView)}] {SUB_MISSION_LABEL3} が見つかりませんでした。");

            _missionSection = rootElement.Q<VisualElement>(MISSION_SECTION)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(StageDetailScreenView)}] {MISSION_SECTION} が見つかりませんでした。");

            _subMission1Root = rootElement.Q<VisualElement>(SUB_MISSION1_ROOT)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(StageDetailScreenView)}] {SUB_MISSION1_ROOT} が見つかりませんでした。");

            _subMission2Root = rootElement.Q<VisualElement>(SUB_MISSION2_ROOT)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(StageDetailScreenView)}] {SUB_MISSION2_ROOT} が見つかりませんでした。");

            _subMission3Root = rootElement.Q<VisualElement>(SUB_MISSION3_ROOT)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(StageDetailScreenView)}] {SUB_MISSION3_ROOT} が見つかりませんでした。");

            _subMission1CheckElement = _subMission1Root.Q<VisualElement>(MISSION_CHECK)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(StageDetailScreenView)}] {SUB_MISSION1_ROOT}/{MISSION_CHECK} が見つかりませんでした。");

            _subMission2CheckElement = _subMission2Root.Q<VisualElement>(MISSION_CHECK)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(StageDetailScreenView)}] {SUB_MISSION2_ROOT}/{MISSION_CHECK} が見つかりませんでした。");

            _subMission3CheckElement = _subMission3Root.Q<VisualElement>(MISSION_CHECK)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(StageDetailScreenView)}] {SUB_MISSION3_ROOT}/{MISSION_CHECK} が見つかりませんでした。");

            _backButton = rootElement.Q<Button>(BACK_BUTTON)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(StageDetailScreenView)}] {BACK_BUTTON} が見つかりませんでした。");

            _sortieButton = rootElement.Q<Button>(SORTIE_BUTTON)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(StageDetailScreenView)}] {SORTIE_BUTTON} が見つかりませんでした。");

            _skillBuildShortcutButton = rootElement.Q<Button>(SKILL_BUILD_SHORTCUT_BUTTON)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(StageDetailScreenView)}] {SKILL_BUILD_SHORTCUT_BUTTON} が見つかりませんでした。");

            VisualElement equippedSkillRow = rootElement.Q<VisualElement>(EQUIPPED_SKILL_ROW)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(StageDetailScreenView)}] {EQUIPPED_SKILL_ROW} が見つかりませんでした。");
            _equippedSkillIcons = equippedSkillRow.Query<Image>().ToList();

            RegisterButtonCallback();
        }

        /// <summary>
        ///     ステージ詳細 DTO を UI に反映します。
        /// </summary>
        public void Apply(in StageDetailDTO dto)
        {
            // フレーバーテキストは「ステージ名\n本文」の2行構成でマスターデータに入力されているため、
            // 生のステージ番号(dto.StageName)は表示せず、フレーバーテキストの1行目を見出しとして使う。
            string[] flavorTextLines = dto.FlavorText?.Split('\n', 2);
            if (flavorTextLines != null && flavorTextLines.Length == 2)
            {
                _stageNameLabel.text = flavorTextLines[0];
                _flavorTextLabel.text = flavorTextLines[1];
            }
            else
            {
                _stageNameLabel.text = dto.StageName;
                _flavorTextLabel.text = dto.FlavorText;
            }

            var rewardSkillBuildText = new StringBuilder("改造ポイント: ");
            rewardSkillBuildText.Append(dto.RewardSkillBuildPoint);
            _rewardSkillBuildLabel.text = rewardSkillBuildText.ToString();
            var rewardSkillUnlockText = new StringBuilder("スキル解放ポイント: ");
            rewardSkillUnlockText.Append(dto.RewardSkillUnlockPoint);
            _rewardSkillUnlockLabel.text = rewardSkillUnlockText.ToString();

            // バトルパートのみミッションセクションを表示する
            _missionSection.style.visibility = dto.IsBattle ? Visibility.Visible : Visibility.Hidden;

            if (dto.IsBattle)
            {
                // ミッション一覧にはメインミッションを表示せず、サブミッションのみを3件まで表示する。
                // 対応するサブミッションが存在しない枠は、テキストを空にするだけでなく枠ごと非表示にする。
                bool hasSubMission1 = dto.SubMissionTexts.Length > 0;
                bool hasSubMission2 = dto.SubMissionTexts.Length > 1;
                bool hasSubMission3 = dto.SubMissionTexts.Length > 2;

                _subMissionLabel1.text = hasSubMission1 ? dto.SubMissionTexts[0] : string.Empty;
                _subMissionLabel2.text = hasSubMission2 ? dto.SubMissionTexts[1] : string.Empty;
                _subMissionLabel3.text = hasSubMission3 ? dto.SubMissionTexts[2] : string.Empty;

                _subMission1Root.style.display = hasSubMission1 ? DisplayStyle.Flex : DisplayStyle.None;
                _subMission2Root.style.display = hasSubMission2 ? DisplayStyle.Flex : DisplayStyle.None;
                _subMission3Root.style.display = hasSubMission3 ? DisplayStyle.Flex : DisplayStyle.None;

                bool[] subMissionCleared = dto.SubMissionCleared;
                _subMission1CheckElement.EnableInClassList(
                    MISSION_CHECK_ACHIEVED_USS_CLASS,
                    subMissionCleared != null && subMissionCleared.Length > 0 && subMissionCleared[0]);
                _subMission2CheckElement.EnableInClassList(
                    MISSION_CHECK_ACHIEVED_USS_CLASS,
                    subMissionCleared != null && subMissionCleared.Length > 1 && subMissionCleared[1]);
                _subMission3CheckElement.EnableInClassList(
                    MISSION_CHECK_ACHIEVED_USS_CLASS,
                    subMissionCleared != null && subMissionCleared.Length > 2 && subMissionCleared[2]);
            }
        }

        /// <summary>
        ///     現在編成中のスキルアイコンをセットします。ステージに依存しないため、パネル構築時などに一度だけ呼び出します。
        /// </summary>
        /// <param name="icons"> 装備スキルのアイコン一覧。空スロットはnullを渡してください。 </param>
        public void SetEquippedSkillIcons(IReadOnlyList<Sprite> icons)
        {
            for (int i = 0; i < _equippedSkillIcons.Count; i++)
            {
                Sprite icon = icons != null && i < icons.Count ? icons[i] : null;
                _equippedSkillIcons[i].sprite = icon;
                _equippedSkillIcons[i].style.display = icon != null ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        public override void Dispose()
        {
            _slideMotionHandle.TryCancel();
            base.Dispose();
            UnregisterButtonCallback();
        }

        /// <summary>
        ///     パネルを画面右外からスライドインさせつつ表示します。
        /// </summary>
        public override ValueTask Show(CancellationToken cancellationToken = default)
        {
            _slideMotionHandle.TryComplete();
            SetPanelTranslateX(SLIDE_OFFSET_X);

            _slideMotionHandle = LMotion.Create(SLIDE_OFFSET_X, 0f, SLIDE_DURATION)
                .WithEase(SLIDE_EASE)
                .Bind(this, static (x, state) => state.SetPanelTranslateX(x));

            return base.Show(cancellationToken);
        }

        /// <summary>
        ///     パネルを画面右外へスライドアウトさせつつ非表示にします。
        /// </summary>
        public override ValueTask Hide(CancellationToken cancellationToken = default)
        {
            _slideMotionHandle.TryComplete();

            _slideMotionHandle = LMotion.Create(0f, SLIDE_OFFSET_X, SLIDE_DURATION)
                .WithEase(SLIDE_EASE)
                .Bind(this, static (x, state) => state.SetPanelTranslateX(x));

            return base.Hide(cancellationToken);
        }

        /// <summary>
        ///     パネルのXオフセットを書き込みます。
        /// </summary>
        /// <param name="x"> 適用する水平方向の移動量(px)。 </param>
        private void SetPanelTranslateX(float x)
        {
            RootElement.style.translate = new Translate(x, 0);
        }

        /// <summary>
        ///     ボタンのコールバックを登録します。
        /// </summary>
        private void RegisterButtonCallback()
        {
            // キャンセル操作で戻れるため、フォーカス移動の対象からは外す。
            _backButton.ExcludeFromNavigation();
            _sortieButton.MakeNavigable();
            _skillBuildShortcutButton.MakeNavigable();

            _backButtonActivation = _backButton.RegisterActivation(HandleBackButtonActivationHandler);
            _sortieButtonActivation = _sortieButton.RegisterActivation(HandleSortieButtonActivationHandler);
            _skillBuildShortcutButtonActivation =
                _skillBuildShortcutButton.RegisterActivation(HandleSkillBuildShortcutButtonActivationHandler);
        }

        /// <summary>
        ///     ボタンのコールバックを解除します。
        /// </summary>
        private void UnregisterButtonCallback()
        {
            _backButtonActivation?.Dispose();
            _sortieButtonActivation?.Dispose();
            _skillBuildShortcutButtonActivation?.Dispose();
        }

        /// <summary>
        ///     戻るボタンが作動したときの処理。
        ///     ステージ詳細画面を閉じるイベントを発火します。
        /// </summary>
        private void HandleBackButtonActivationHandler()
        {
            OutGameUIEvent.OnStageDetailClosed?.Invoke();
        }

        /// <summary>
        ///     出撃ボタンが作動したときの処理。
        ///     ステージタイプに応じた出撃イベントを発火します。
        /// </summary>
        private void HandleSortieButtonActivationHandler()
        {
            OutGameUIEvent.OnSortieRequested?.Invoke();
        }

        /// <summary>
        ///     改造ショートカットボタンが作動したときの処理。
        ///     改造(SkillBuild)画面への遷移イベントを発火します。
        /// </summary>
        private void HandleSkillBuildShortcutButtonActivationHandler()
        {
            OutGameUIEvent.OnShownSkillBuildScreen?.Invoke();
        }

        private const string STAGE_NAME_LABEL = "StageNameLabel";
        private const string FLAVOR_TEXT_LABEL = "FlavorTextLabel";
        private const string REWARD_SKILL_BUILD_LABEL = "RewardSkillBuildLabel";
        private const string REWARD_SKILL_UNLOCK_LABEL = "RewardSkillUnlockLabel";
        private const string SUB_MISSION_LABEL1 = "SubMissionLabel1";
        private const string SUB_MISSION_LABEL2 = "SubMissionLabel2";
        private const string SUB_MISSION_LABEL3 = "SubMissionLabel3";
        private const string MISSION_SECTION = "MissionSection";
        private const string SUB_MISSION1_ROOT = "SubMission1Root";
        private const string SUB_MISSION2_ROOT = "SubMission2Root";
        private const string SUB_MISSION3_ROOT = "SubMission3Root";
        private const string MISSION_CHECK = "MissionCheck";
        private const string MISSION_CHECK_ACHIEVED_USS_CLASS = "mission-check-mission-achieved";
        private const string BACK_BUTTON = "BackButton";
        private const string SORTIE_BUTTON = "SortieButton";
        private const string SKILL_BUILD_SHORTCUT_BUTTON = "SkillBuildShortcutButton";
        private const string EQUIPPED_SKILL_ROW = "EquippedSkillRow";
        /// <summary> パネルのスライドインにかかる時間(秒)。 </summary>
        private const float SLIDE_DURATION = 0.25f;
        /// <summary> パネルのスライド開始位置(画面右外側へのオフセット、px)。パネル幅(500px)ぶん逃がす。 </summary>
        private const float SLIDE_OFFSET_X = 500f;
        /// <summary> パネルのスライドのイージング。 </summary>
        private const Ease SLIDE_EASE = Ease.OutCirc;

        private readonly Label _stageNameLabel;
        private readonly Label _flavorTextLabel;
        private readonly Label _rewardSkillBuildLabel;
        private readonly Label _rewardSkillUnlockLabel;
        private readonly Label _subMissionLabel1;
        private readonly Label _subMissionLabel2;
        private readonly Label _subMissionLabel3;
        private readonly VisualElement _missionSection;
        private readonly VisualElement _subMission1Root;
        private readonly VisualElement _subMission2Root;
        private readonly VisualElement _subMission3Root;
        private readonly VisualElement _subMission1CheckElement;
        private readonly VisualElement _subMission2CheckElement;
        private readonly VisualElement _subMission3CheckElement;
        /// <inheritdoc />
        /// <remarks> ノードを選択して詳細が開いたら、そのまま出撃できるようにする。 </remarks>
        protected override VisualElement InitialFocusElement => _sortieButton;

        /// <inheritdoc />
        protected override VisualElement CancelTargetElement => _backButton;

        private readonly Button _backButton;
        private readonly Button _sortieButton;
        private readonly Button _skillBuildShortcutButton;
        private readonly List<Image> _equippedSkillIcons;
        private MotionHandle _slideMotionHandle;
        private IDisposable _backButtonActivation;
        private IDisposable _sortieButtonActivation;
        private IDisposable _skillBuildShortcutButtonActivation;
    }
}
