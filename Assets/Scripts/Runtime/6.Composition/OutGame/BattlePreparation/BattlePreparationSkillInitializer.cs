using KillChord.Runtime.Adaptor.OutGame.BattlePreparation;
using KillChord.Runtime.Adaptor.OutGame.Skill;
using KillChord.Runtime.Composition.OutGame.Bootstrap;
using KillChord.Runtime.Domain.OutGame.SkillBuild;
using KillChord.Runtime.View.OutGame.BattlePreparation;
using KillChord.Runtime.View.OutGame.Screen;
using SymphonyFrameWork.System.ServiceLocate;
using System.Collections.Generic;
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
        ///     装備スキル構成の変更を購読し、現在値を表示へ反映します。
        /// </summary>
        /// <returns> 購読に成功した場合はtrue。 </returns>
        public override bool Ready()
        {
            if (!_isInitialized)
            {
                return false;
            }

            _skillBuildDefinition.OnEquippedSkillsChanged += HandleEquippedSkillsChangedHandler;
            _isSubscribed = true;
            PushCurrentSkills();
            return true;
        }

        /// <summary>
        ///     イベント購読と表示用コンポーネントを解放します。
        /// </summary>
        public override void Shutdown()
        {
            if (_isSubscribed && _skillBuildDefinition != null)
            {
                _skillBuildDefinition.OnEquippedSkillsChanged -= HandleEquippedSkillsChangedHandler;
            }

            _battlePreparationScreen?.Unbind();
            _viewModel?.Dispose();
            _viewModel = null;
            _presenter = null;
            _battlePreparationScreen = null;
            _skillBuildDefinition = null;
            _isInitialized = false;
            _isSubscribed = false;
        }

        private SkillBuildDefinition _skillBuildDefinition;
        private BattlePreparationScreen _battlePreparationScreen;
        private BattlePreparationSkillViewModel _viewModel;
        private BattlePreparationSkillPresenter _presenter;
        private bool _isInitialized;
        private bool _isSubscribed;

        /// <summary>
        ///     装備スキル構成の変更を表示へ反映します。
        /// </summary>
        /// <param name="equippedSkills"> 変更後の装備スキル構成です。 </param>
        private void HandleEquippedSkillsChangedHandler(
            IReadOnlyList<EquippedSkill> equippedSkills)
        {
            _presenter?.Push(equippedSkills);
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
