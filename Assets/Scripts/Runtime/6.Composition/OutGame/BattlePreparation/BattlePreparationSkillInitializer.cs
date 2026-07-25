using KillChord.Runtime.Adaptor.OutGame.BattlePreparation;
using KillChord.Runtime.Adaptor.OutGame.Skill;
using KillChord.Runtime.Composition.OutGame.Bootstrap;
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

            if (!ServiceLocator.TryGetInstance(out _outGameUIEvent))
            {
#if UNITY_EDITOR
                Debug.LogError(
                    $"[{nameof(BattlePreparationSkillInitializer)}] {nameof(OutGameUIEvent)} が取得できませんでした。",
                    this);
#endif
                return false;
            }

            _viewModel = new BattlePreparationSkillViewModel();
            _presenter = new BattlePreparationSkillPresenter(
                _viewModel,
                new SkillEffectDescriptionFormatter());
            _battlePreparationScreen.Bind(_viewModel);
            PushCurrentSkills();
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

            _outGameUIEvent.OnShownBattlePreparationScreen +=
                HandleShownBattlePreparationScreenHandler;
            _isSubscribed = true;
            return true;
        }

        /// <summary>
        ///     イベント購読と表示用コンポーネントを解放します。
        /// </summary>
        public override void Shutdown()
        {
            if (_isSubscribed && _outGameUIEvent != null)
            {
                _outGameUIEvent.OnShownBattlePreparationScreen -=
                    HandleShownBattlePreparationScreenHandler;
            }

            _battlePreparationScreen?.Unbind();
            _viewModel = null;
            _presenter = null;
            _battlePreparationScreen = null;
            _skillBuildDefinition = null;
            _outGameUIEvent = null;
            _isInitialized = false;
            _isSubscribed = false;
        }

        private SkillBuildDefinition _skillBuildDefinition;
        private BattlePreparationScreen _battlePreparationScreen;
        private BattlePreparationSkillViewModel _viewModel;
        private BattlePreparationSkillPresenter _presenter;
        private OutGameUIEvent _outGameUIEvent;
        private bool _isInitialized;
        private bool _isSubscribed;

        /// <summary>
        ///     戦闘準備画面の表示時に最新の装備状態を反映します。
        /// </summary>
        /// <param name="targetSceneName"> 遷移先シーン名です。 </param>
        private void HandleShownBattlePreparationScreenHandler(string targetSceneName)
        {
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
