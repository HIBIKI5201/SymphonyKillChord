using KillChord.Runtime.Adaptor.OutGame.StageSelect;
using KillChord.Runtime.View.OutGame.Screen;
using System.Text;
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

            _mainMissionLabel = rootElement.Q<Label>(MAIN_MISSION_LABEL)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(StageDetailScreenView)}] {MAIN_MISSION_LABEL} が見つかりませんでした。");

            _subMissionLabel1 = rootElement.Q<Label>(SUB_MISSION_LABEL1)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(StageDetailScreenView)}] {SUB_MISSION_LABEL1} が見つかりませんでした。");

            _subMissionLabel2 = rootElement.Q<Label>(SUB_MISSION_LABEL2)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(StageDetailScreenView)}] {SUB_MISSION_LABEL2} が見つかりませんでした。");

            _missionSection = rootElement.Q<VisualElement>(MISSION_SECTION)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(StageDetailScreenView)}] {MISSION_SECTION} が見つかりませんでした。");

            _backButton = rootElement.Q<Button>(BACK_BUTTON)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(StageDetailScreenView)}] {BACK_BUTTON} が見つかりませんでした。");

            _sortieButton = rootElement.Q<Button>(SORTIE_BUTTON)
                ?? throw new System.ArgumentNullException(
                    $"[{nameof(StageDetailScreenView)}] {SORTIE_BUTTON} が見つかりませんでした。");

            RegisterButtonCallback();
        }

        /// <summary>
        ///     ステージ詳細 DTO を UI に反映します。
        /// </summary>
        public void Apply(in StageDetailDTO dto)
        {
            _stageNameLabel.text = dto.StageName;
            _flavorTextLabel.text = dto.FlavorText;

            var rewardSkillBuildText = new StringBuilder("改造ポイント: ");
            rewardSkillBuildText.Append(dto.RewardSkillBuildPoint);
            _rewardSkillBuildLabel.text = rewardSkillBuildText.ToString();
            var rewardSkillUnlockText = new StringBuilder("スキル解放ポイント: ");
            rewardSkillUnlockText.Append(dto.RewardSkillUnlockPoint);
            _rewardSkillUnlockLabel.text = rewardSkillUnlockText.ToString();

            // バトルパートのみミッションセクションを表示する
            _missionSection.style.display = dto.IsBattle ? DisplayStyle.Flex : DisplayStyle.None;

            if (dto.IsBattle)
            {
                _mainMissionLabel.text = dto.MainMissionText;

                // ミッション定義情報からサブミッションは2つしか、現状確認していないため配列のインデックスを直接指定している。
                _subMissionLabel1.text = dto.SubMissionTexts.Length > 0 ? dto.SubMissionTexts[0] : string.Empty;
                _subMissionLabel2.text = dto.SubMissionTexts.Length > 1 ? dto.SubMissionTexts[1] : string.Empty;
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            UnregisterButtonCallback();
        }

        /// <summary>
        ///     ボタンのコールバックを登録します。
        /// </summary>
        private void RegisterButtonCallback()
        {
            _backButton.RegisterCallback<ClickEvent>(OnBackButtonClicked);
            _sortieButton.RegisterCallback<ClickEvent>(OnSortieButtonClicked);
        }

        /// <summary>
        ///     ボタンのコールバックを解除します。
        /// </summary>
        private void UnregisterButtonCallback()
        {
            _backButton.UnregisterCallback<ClickEvent>(OnBackButtonClicked);
            _sortieButton.UnregisterCallback<ClickEvent>(OnSortieButtonClicked);
        }

        /// <summary>
        ///     戻るボタンがクリックされたときの処理。
        ///     ステージ詳細画面を閉じるイベントを発火します。
        /// </summary>
        private void OnBackButtonClicked(ClickEvent evt)
        {
            OutGameUIEvent.OnStageDetailClosed?.Invoke();
        }

        /// <summary>
        ///     出撃ボタンがクリックされたときの処理。
        ///     戦闘準備画面を表示するイベントを発火します。
        /// </summary>
        private void OnSortieButtonClicked(ClickEvent evt)
        {
            OutGameUIEvent.OnShownBattlePreparationScreen?.Invoke();
        }

        private const string STAGE_NAME_LABEL = "StageNameLabel";
        private const string FLAVOR_TEXT_LABEL = "FlavorTextLabel";
        private const string REWARD_SKILL_BUILD_LABEL = "RewardSkillBuildLabel";
        private const string REWARD_SKILL_UNLOCK_LABEL = "RewardSkillUnlockLabel";
        private const string MAIN_MISSION_LABEL = "MainMissionLabel";
        private const string SUB_MISSION_LABEL1 = "SubMissionLabel1";
        private const string SUB_MISSION_LABEL2 = "SubMissionLabel2";
        private const string MISSION_SECTION = "MissionSection";
        private const string BACK_BUTTON = "BackButton";
        private const string SORTIE_BUTTON = "SortieButton";

        private readonly Label _stageNameLabel;
        private readonly Label _flavorTextLabel;
        private readonly Label _rewardSkillBuildLabel;
        private readonly Label _rewardSkillUnlockLabel;
        private readonly Label _mainMissionLabel;
        private readonly Label _subMissionLabel1;
        private readonly Label _subMissionLabel2;
        private readonly VisualElement _missionSection;
        private readonly Button _backButton;
        private readonly Button _sortieButton;
    }
}
