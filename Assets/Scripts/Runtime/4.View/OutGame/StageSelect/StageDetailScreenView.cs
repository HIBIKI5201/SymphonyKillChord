using KillChord.Runtime.Adaptor.OutGame.StageSelect;
using KillChord.Runtime.View.OutGame.Common;
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
            VisualElement topHeader = rootElement.Q<VisualElement>(TOP_HEADER)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(StageDetailScreenView)}] {TOP_HEADER} が見つかりませんでした。");

            _stageNameLabel = topHeader.Q<Label>(STAGE_NAME_LABEL)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(StageDetailScreenView)}] {TOP_HEADER}/{STAGE_NAME_LABEL} が見つかりませんでした。");

            _flavorTextLabel = topHeader.Q<Label>(FLAVOR_TEXT_LABEL)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(StageDetailScreenView)}] {TOP_HEADER}/{FLAVOR_TEXT_LABEL} が見つかりませんでした。");

            VisualElement reward = rootElement.Q<VisualElement>(REWARD)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(StageDetailScreenView)}] {REWARD} が見つかりませんでした。");

            VisualElement firstClearReward = reward.Q<VisualElement>(REWARD_FIRST_CLEAR)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(StageDetailScreenView)}] {REWARD}/{REWARD_FIRST_CLEAR} が見つかりませんでした。");

            VisualElement successReward = reward.Q<VisualElement>(REWARD_SUCCESS)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(StageDetailScreenView)}] {REWARD}/{REWARD_SUCCESS} が見つかりませんでした。");

            _rewardSkillUnlockLabel = firstClearReward.Q<Label>(REWARD_POINT)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(StageDetailScreenView)}] {REWARD}/{REWARD_FIRST_CLEAR}/{REWARD_POINT} が見つかりませんでした。");

            _rewardSkillBuildLabel = successReward.Q<Label>(REWARD_POINT)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(StageDetailScreenView)}] {REWARD}/{REWARD_SUCCESS}/{REWARD_POINT} が見つかりませんでした。");

            _missionSection = rootElement.Q<VisualElement>(MISSION)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(StageDetailScreenView)}] {MISSION} が見つかりませんでした。");

            _missionHeadingRoot = _missionSection.Q<VisualElement>(MISSION_MAIN)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(StageDetailScreenView)}] {MISSION}/{MISSION_MAIN} が見つかりませんでした。");

            _missionHeadingLabel = _missionHeadingRoot.Q<Label>(MISSION_NAME)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(StageDetailScreenView)}] {MISSION}/{MISSION_MAIN}/{MISSION_NAME} が見つかりませんでした。");

            VisualElement missionList = _missionSection.Q<VisualElement>(MISSION_LIST)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(StageDetailScreenView)}] {MISSION}/{MISSION_LIST} が見つかりませんでした。");

            _subMission1Root = missionList.Q<VisualElement>(MISSION_MAIN)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(StageDetailScreenView)}] {MISSION}/{MISSION_LIST}/{MISSION_MAIN} が見つかりませんでした。");

            _subMission2Root = missionList.Q<VisualElement>(MISSION_LIST_SUB1)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(StageDetailScreenView)}] {MISSION}/{MISSION_LIST}/{MISSION_LIST_SUB1} が見つかりませんでした。");

            _subMission3Root = missionList.Q<VisualElement>(MISSION_LIST_SUB2)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(StageDetailScreenView)}] {MISSION}/{MISSION_LIST}/{MISSION_LIST_SUB2} が見つかりませんでした。");

            _subMissionLabel1 = _subMission1Root.Q<Label>(SUB_MISSION_LABEL1)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(StageDetailScreenView)}] {SUB_MISSION_LABEL1} が見つかりませんでした。");

            _subMissionLabel2 = _subMission2Root.Q<Label>(SUB_MISSION_LABEL2)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(StageDetailScreenView)}] {SUB_MISSION_LABEL2} が見つかりませんでした。");

            _subMissionLabel3 = _subMission3Root.Q<Label>(SUB_MISSION_LABEL3)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(StageDetailScreenView)}] {SUB_MISSION_LABEL3} が見つかりませんでした。");

            _subMission1CheckElement = _subMission1Root.Q<VisualElement>(MISSION_CHECK)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(StageDetailScreenView)}] {MISSION}/{MISSION_LIST}/{MISSION_MAIN}/{MISSION_CHECK} が見つかりませんでした。");

            _subMission2CheckElement = _subMission2Root.Q<VisualElement>(MISSION_CHECK)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(StageDetailScreenView)}] {MISSION}/{MISSION_LIST}/{MISSION_LIST_SUB1}/{MISSION_CHECK} が見つかりませんでした。");

            _subMission3CheckElement = _subMission3Root.Q<VisualElement>(MISSION_CHECK)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(StageDetailScreenView)}] {MISSION}/{MISSION_LIST}/{MISSION_LIST_SUB2}/{MISSION_CHECK} が見つかりませんでした。");

            _sortieButton = rootElement.Q<Button>(SORTIE_BUTTON)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(StageDetailScreenView)}] {SORTIE_BUTTON} が見つかりませんでした。");

            _skillBuildShortcutButton = rootElement.Q<Button>(SKILL_BUILD_SHORTCUT_BUTTON)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(StageDetailScreenView)}] {SKILL_BUILD_SHORTCUT_BUTTON} が見つかりませんでした。");

            VisualElement skillBuild = rootElement.Q<VisualElement>(SKILL_BUILD)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(StageDetailScreenView)}] {SKILL_BUILD} が見つかりませんでした。");

            _equippedSkillRow = skillBuild.Q<VisualElement>(SKILL_SLOT)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(StageDetailScreenView)}] {SKILL_BUILD}/{SKILL_SLOT} が見つかりませんでした。");
            _equippedSkillSlots = _equippedSkillRow.Query<VisualElement>(className: EQUIPPED_SKILL_SLOT_USS_CLASS).ToList();
            _equippedSkillNames = new List<Label>(_equippedSkillSlots.Count);
            _equippedSkillIcons = new List<Image>(_equippedSkillSlots.Count);
            _equippedSkillCommandRows = new List<VisualElement>(_equippedSkillSlots.Count);
            for (int i = 0; i < _equippedSkillSlots.Count; i++)
            {
                _equippedSkillNames.Add(_equippedSkillSlots[i].Q<Label>(className: EQUIPPED_SKILL_NAME_USS_CLASS));
                _equippedSkillIcons.Add(_equippedSkillSlots[i].Q<Image>(className: EQUIPPED_SKILL_ICON_USS_CLASS));
                _equippedSkillCommandRows.Add(
                    _equippedSkillSlots[i].Q<VisualElement>(className: EQUIPPED_SKILL_COMMAND_ROW_USS_CLASS));
            }

            RegisterButtonCallback();
        }

        /// <summary>
        ///     強制出撃中の装備変更と詳細パネルのキャンセルを禁止します。
        /// </summary>
        /// <param name="isForced"> 強制出撃中の場合はtrueです。 </param>
        public void SetForcedSortieMode(bool isForced)
        {
            _isForcedSortieMode = isForced;
            _skillBuildShortcutButton.SetEnabled(!isForced);
            if (isForced)
            {
                SetInitialFocusElement(_sortieButton);
            }
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

            // 初回報酬ボックス: 解放P「現在値 → 加算後」、成功報酬ボックス: 改造P「現在値 → 加算後」。
            _rewardSkillUnlockLabel.text = BuildRewardArrowText(
                dto.CurrentSkillUnlockPoint, dto.FirstClearRewardSkillUnlockPoint);
            _rewardSkillBuildLabel.text = BuildRewardArrowText(
                dto.CurrentSkillBuildPoint, dto.SuccessRewardSkillBuildPoint);

            // バトルパートのみミッション見出し・ミッションセクションを表示する
            _missionHeadingRoot.style.display = dto.IsBattle ? DisplayStyle.Flex : DisplayStyle.None;
            _missionSection.style.visibility = dto.IsBattle ? Visibility.Visible : Visibility.Hidden;

            if (dto.IsBattle)
            {
                _missionHeadingLabel.text = dto.MainMissionText;
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
        ///     「現在値 → 現在値+加算量」形式の報酬表示テキストを組み立てます。
        /// </summary>
        /// <param name="currentPoint"> 現在のポイント。 </param>
        /// <param name="rewardPoint"> 加算されるポイント。 </param>
        /// <returns> 「現在値 → 加算後の値」形式の文字列。 </returns>
        private static string BuildRewardArrowText(int currentPoint, int rewardPoint)
        {
            return $"{currentPoint} {REWARD_ARROW} {currentPoint + rewardPoint}";
        }

        /// <summary>
        ///     現在編成中のスキルの名前・アイコン・発動コマンドをセットします。
        ///     改造画面の編成スロットと同じく、空スロットは「＋」プレースホルダー表示のまま残します。
        ///     ステージに依存しないため、パネル構築時などに一度だけ呼び出します。
        /// </summary>
        /// <param name="names"> 装備スキルの表示名一覧。空スロットはnullを渡してください。 </param>
        /// <param name="icons"> 装備スキルのアイコン一覧。空スロットはnullを渡してください。 </param>
        /// <param name="commandColors">
        ///     各スキルの発動コマンド色一覧(インゲームのBeatType配色と対応)。空スロットまたは発動コマンドなしはnullを渡してください。
        /// </param>
        /// <param name="hexSprite"> 発動コマンドの菱形表示に使う形状スプライト。 </param>
        public void SetEquippedSkillIcons(
            IReadOnlyList<string> names,
            IReadOnlyList<Sprite> icons,
            IReadOnlyList<Color[]> commandColors,
            Sprite hexSprite)
        {
            for (int i = 0; i < _equippedSkillIcons.Count; i++)
            {
                // 現在のスキル編成のスロット数(icons/namesの件数)を超える枠は、UI上も存在しないものとして隠す。
                bool isSlotInCurrentFormation = icons != null && i < icons.Count;
                _equippedSkillSlots[i].style.display =
                    isSlotInCurrentFormation ? DisplayStyle.Flex : DisplayStyle.None;
                if (!isSlotInCurrentFormation) { continue; }

                Sprite icon = icons[i];
                string skillName = names != null && i < names.Count ? names[i] : null;
                _equippedSkillIcons[i].sprite = icon;
                _equippedSkillNames[i].text = skillName;
                _equippedSkillSlots[i].EnableInClassList(EQUIPPED_SKILL_SLOT_FILLED_USS_CLASS, icon != null);

                Color[] steps = icon != null && commandColors != null && i < commandColors.Count
                    ? commandColors[i]
                    : null;
                ComboHexRowBuilder.Build(
                    _equippedSkillCommandRows[i], steps, hexSprite, EQUIPPED_SKILL_COMMAND_HEX_USS_CLASS);
            }

            // 装備数によらず枠同士の間隔(margin基準)を常に一定にするため、常に中央揃えにする。
            _equippedSkillRow.style.justifyContent = Justify.Center;
        }

        public override void Dispose()
        {
            _slideMotionHandle.TryCancel();
            base.Dispose();
            UnregisterButtonCallback();
        }

        /// <summary>
        ///     パネルを画面右外からスライドインさせつつ表示します。
        ///     表示中はB(キャンセル)を押すまでパネル外へフォーカスが移動しないよう封じ込めます。
        /// </summary>
        public override ValueTask Show(CancellationToken cancellationToken = default)
        {
            _navigationScope.Activate(RootElement);

            _slideMotionHandle.TryComplete();
            SetPanelTranslateX(SLIDE_OFFSET_X);

            _slideMotionHandle = LMotion.Create(SLIDE_OFFSET_X, 0f, SLIDE_DURATION)
                .WithEase(SLIDE_EASE)
                .Bind(this, static (x, state) => state.SetPanelTranslateX(x));

            return base.Show(cancellationToken);
        }

        /// <summary>
        ///     パネルを画面右外へスライドアウトさせつつ非表示にし、フォーカスの封じ込めを解除します。
        /// </summary>
        public override ValueTask Hide(CancellationToken cancellationToken = default)
        {
            _navigationScope.Deactivate();

            _slideMotionHandle.TryComplete();

            _slideMotionHandle = LMotion.Create(0f, SLIDE_OFFSET_X, SLIDE_DURATION)
                .WithEase(SLIDE_EASE)
                .Bind(this, static (x, state) => state.SetPanelTranslateX(x));

            return base.Hide(cancellationToken);
        }

        /// <summary>
        ///     パネルをフェードなしで即座に非表示にし、フォーカスの封じ込めを解除します。
        /// </summary>
        public override void HideImmediately()
        {
            _navigationScope.Deactivate();
            base.HideImmediately();
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
            _sortieButton.MakeNavigable();
            _skillBuildShortcutButton.MakeNavigable();

            _sortieButtonActivation = _sortieButton.RegisterActivation(HandleSortieButtonActivationHandler);
            _skillBuildShortcutButtonActivation =
                _skillBuildShortcutButton.RegisterActivation(HandleSkillBuildShortcutButtonActivationHandler);
            // BackButtonが無いため、パネル自体をキャンセル対象(CancelTargetElement)として扱う。
            _cancelActivation = RootElement.RegisterActivation(HandleCancelActivationHandler);
        }

        /// <summary>
        ///     ボタンのコールバックを解除します。
        /// </summary>
        private void UnregisterButtonCallback()
        {
            _sortieButtonActivation?.Dispose();
            _skillBuildShortcutButtonActivation?.Dispose();
            _cancelActivation?.Dispose();
        }

        /// <summary>
        ///     コントローラー/キーボードのキャンセル操作でパネルを閉じます。
        /// </summary>
        private void HandleCancelActivationHandler()
        {
            if (_isForcedSortieMode) { return; }

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
            if (_isForcedSortieMode) { return; }

            OutGameUIEvent.OnShownSkillBuildScreen?.Invoke();
        }

        private const string TOP_HEADER = "TopHeader";
        private const string STAGE_NAME_LABEL = "StageNameLabel";
        private const string FLAVOR_TEXT_LABEL = "FlavorTextLabel";
        private const string REWARD = "Reward";
        private const string REWARD_FIRST_CLEAR = "FirstClear";
        private const string REWARD_SUCCESS = "Success";
        private const string REWARD_POINT = "Point";
        private const string SUB_MISSION_LABEL1 = "SubMissionLabel1";
        private const string SUB_MISSION_LABEL2 = "SubMissionLabel2";
        private const string SUB_MISSION_LABEL3 = "SubMissionLabel3";
        private const string MISSION = "Mission";
        private const string MISSION_MAIN = "Main";
        private const string MISSION_NAME = "Name";
        private const string MISSION_LIST = "MissionList";
        private const string MISSION_LIST_SUB1 = "Sub1";
        private const string MISSION_LIST_SUB2 = "Sub2";
        private const string MISSION_CHECK = "MissionCheck";
        private const string MISSION_CHECK_ACHIEVED_USS_CLASS = "mission-check-mission-achieved";
        private const string SORTIE_BUTTON = "SortieButton";
        private const string SKILL_BUILD_SHORTCUT_BUTTON = "SkillBuildShortcutButton";
        private const string SKILL_BUILD = "SkillBuild";
        private const string SKILL_SLOT = "SkillSlot";
        private const string EQUIPPED_SKILL_SLOT_USS_CLASS = "equipped-skill-slot";
        private const string EQUIPPED_SKILL_SLOT_FILLED_USS_CLASS = "is-filled";
        private const string EQUIPPED_SKILL_NAME_USS_CLASS = "equipped-skill-name";
        private const string EQUIPPED_SKILL_ICON_USS_CLASS = "equipped-skill-icon";
        private const string EQUIPPED_SKILL_COMMAND_ROW_USS_CLASS = "equipped-skill-command-row";
        private const string EQUIPPED_SKILL_COMMAND_HEX_USS_CLASS = "equipped-skill-command-hex";
        /// <summary> 報酬表示の矢印記号。 </summary>
        private const string REWARD_ARROW = "→";
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
        private readonly VisualElement _missionHeadingRoot;
        private readonly Label _missionHeadingLabel;
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
        /// <remarks>
        ///     BackButtonが無いため、パネル自体(RootElement)をキャンセル対象にし、
        ///     コントローラー/キーボードのキャンセル操作でも閉じられるようにする。
        /// </remarks>
        protected override VisualElement CancelTargetElement => RootElement;

        private readonly Button _sortieButton;
        private readonly Button _skillBuildShortcutButton;
        private readonly VisualElement _equippedSkillRow;
        private readonly List<VisualElement> _equippedSkillSlots;
        private readonly List<Label> _equippedSkillNames;
        private readonly List<Image> _equippedSkillIcons;
        private readonly List<VisualElement> _equippedSkillCommandRows;
        private MotionHandle _slideMotionHandle;
        private IDisposable _sortieButtonActivation;
        private IDisposable _skillBuildShortcutButtonActivation;
        private IDisposable _cancelActivation;
        private bool _isForcedSortieMode;
        /// <summary> ウィンドウ表示中、フォーカスをウィンドウ内へ閉じ込めます。 </summary>
        private readonly ModalNavigationScope _navigationScope = new();
    }
}
