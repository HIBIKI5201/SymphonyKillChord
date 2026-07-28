using KillChord.Runtime.Adaptor.OutGame.BattlePreparation;
using KillChord.Runtime.Adaptor.OutGame.Screen;
using KillChord.Runtime.Adaptor.OutGame.Skill;
using KillChord.Runtime.Composition.OutGame.Bootstrap;
using KillChord.Runtime.Domain.OutGame.Screen;
using KillChord.Runtime.Domain.OutGame.SkillBuild;
using KillChord.Runtime.View.OutGame.BattlePreparation;
using KillChord.Runtime.View.OutGame.Screen;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition.OutGame.BattlePreparation
{
    /// <summary>
    ///     戦闘準備画面の装備スキル表示に必要な依存を解決します。
    /// </summary>
    public sealed class BattlePreparationSkillInitializer : OutGameInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(BattlePreparationSkillInitializer);

        /// <summary> 実行順です。 </summary>
        public override int Order => 140;

        /// <summary>
        ///     表示用コンポーネントを構築して接続します。
        /// </summary>
        /// <returns> 構築に成功した場合はtrue。 </returns>
        public override bool Build()
        {
            if (!ServiceLocator.TryGetInstance(out _skillBuildDefinition))
            {
#if UNITY_EDITOR
                Debug.LogError(
                    $"[{nameof(BattlePreparationSkillInitializer)}] {nameof(SkillBuildDefinition)} が取得できませんでした。",
                    this);
#endif
                return false;
            }

            if (!ServiceLocator.TryGetInstance(out _battlePreparationScreen))
            {
#if UNITY_EDITOR
                Debug.LogError(
                    $"[{nameof(BattlePreparationSkillInitializer)}] {nameof(BattlePreparationScreen)} が取得できませんでした。",
                    this);
#endif
                return false;
            }

            if (!ServiceLocator.TryGetInstance(out _screenLifecycleSignal))
            {
#if UNITY_EDITOR
                Debug.LogError(
                    $"[{nameof(BattlePreparationSkillInitializer)}] {nameof(IScreenLifecycleSignal)} が取得できませんでした。",
                    this);
#endif
                return false;
            }

            _viewModel = new BattlePreparationSkillViewModel();
            SkillEffectDescriptionFormatter effectDescriptionFormatter = new SkillEffectDescriptionFormatter();
            SkillDisplayTextFormatter textFormatter = new SkillDisplayTextFormatter(effectDescriptionFormatter);
            _presenter = new BattlePreparationSkillPresenter(
                _viewModel,
                textFormatter);
            _battlePreparationScreen.Bind(_viewModel);
            _isInitialized = true;
            return true;
        }

        /// <summary>
        ///     画面表示イベントを購読します。
        /// </summary>
        /// <returns> 購読に成功した場合はtrue。 </returns>
        public override bool Ready()
        {
            if (!_isInitialized)
            {
                return false;
            }

            _screenLifecycleSignal.OnScreenWillShow += HandleScreenWillShowHandler;
            _isSubscribed = true;
            return true;
        }

        /// <summary>
        ///     イベント購読と表示用コンポーネントを解放します。
        /// </summary>
        public override void Shutdown()
        {
            if (_isSubscribed && _screenLifecycleSignal != null)
            {
                _screenLifecycleSignal.OnScreenWillShow -= HandleScreenWillShowHandler;
            }

            _battlePreparationScreen?.Unbind();
            _viewModel?.Dispose();
            _viewModel = null;
            _presenter = null;
            _battlePreparationScreen = null;
            _skillBuildDefinition = null;
            _screenLifecycleSignal = null;
            _isInitialized = false;
            _isSubscribed = false;
        }

        private SkillBuildDefinition _skillBuildDefinition;
        private BattlePreparationScreen _battlePreparationScreen;
        private BattlePreparationSkillViewModel _viewModel;
        private BattlePreparationSkillPresenter _presenter;
        private IScreenLifecycleSignal _screenLifecycleSignal;
        private bool _isInitialized;
        private bool _isSubscribed;

        /// <summary>
        ///     戦闘準備画面の表示直前に最新の装備状態を反映します。
        /// </summary>
        private void HandleScreenWillShowHandler(ScreenId screenId)
        {
            if (screenId != ScreenId.BattlePreparation) { return; }

            PushCurrentSkills();
        }

        /// <summary>
        ///     現在の装備スキル一覧を表示へ反映します。
        /// </summary>
        private void PushCurrentSkills()
        {
            _presenter?.Push(_skillBuildDefinition.EquippedSkills);
        }
    }
}
