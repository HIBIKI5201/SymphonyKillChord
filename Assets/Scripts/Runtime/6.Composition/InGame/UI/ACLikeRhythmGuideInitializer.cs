using KillChord.Runtime.Adaptor.InGame.Battle;
using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.Adaptor.InGame.PostEffect;
using KillChord.Runtime.Adaptor.InGame.Target;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Composition.InGame.Bootstrap;
using KillChord.Runtime.Composition.InGame.Music;
using KillChord.Runtime.Composition.InGame.Player;
using KillChord.Runtime.Composition.InGame.Sequence;
using KillChord.Runtime.Composition.InGame.Target;
using KillChord.Runtime.InfraStructure.InGame.Music;
using KillChord.Runtime.View.InGame.Music;
using KillChord.Runtime.View.InGame.PostEffect;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.UI
{
    /// <summary>
    ///     AC風リズムガイドを初期化するモジュールです。
    /// </summary>
    public sealed class ACLikeRhythmGuideInitializer : InGameInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(ACLikeRhythmGuideInitializer);

        /// <summary> 実行順です。 </summary>
        public override int Order => 800;

        /// <summary>
        ///     他モジュールへ結合してリズムガイドを初期化する。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Ready()
        {
            if (_rhythmJudgmentDefinitionAsset == null || _rhythmGuideView == null)
            {
                Debug.LogError($"[{nameof(ACLikeRhythmGuideInitializer)}] リズムガイド参照が不足しています。", this);
                return false;
            }

            if (ServiceLocator.GetInstance<MusicSyncModuleContainer>() == null
                || ServiceLocator.GetInstance<TargetSystemModuleContainer>() == null)
            {
                Debug.LogError($"[{nameof(ACLikeRhythmGuideInitializer)}] 必要なContainerが見つかりません。", this);
                return false;
            }

            if (!Initialize())
            {
                return false;
            }

            RegisterGameplayControllable();
            return true;
        }

        /// <summary>
        ///     リズムガイド機能を初期化する。
        /// </summary>
        /// <returns> 初期化に成功した場合はtrueです。 </returns>
        public bool Initialize()
        {
            Debug.Assert(_rhythmJudgmentDefinitionAsset != null, "RhythmJudgmentDefinitionAsset の参照が未設定です。RhythmJudgmentDefinitionAsset を設定してください。");
            Debug.Assert(_rhythmGuideView != null, "RhythmGuideView の参照が未設定です。RhythmGuideView を設定してください。");

            IMusicSyncService musicSyncService = ServiceLocator.GetInstance<MusicSyncModuleContainer>()?.MusicSyncService;

            if (musicSyncService == null)
            {
                Debug.LogError($"[{nameof(ACLikeRhythmGuideInitializer)}] {nameof(IMusicSyncService)} が見つかりません。MusicSyncInitializer が先に初期化されているか確認してください。", this);
                return false;
            }

            TargetSystemController targetingSystem = ServiceLocator.GetInstance<TargetSystemModuleContainer>()?.TargetSystemController;

            if (targetingSystem == null)
            {
                Debug.LogError($"[{nameof(ACLikeRhythmGuideInitializer)}] {nameof(TargetSystemController)} が見つかりません。TargetSystemController が登録されているか確認してください。", this);
                return false;
            }

            var missionContainer = ServiceLocator.GetInstance<KillChord.Runtime.Composition.InGame.Mission.MissionModuleContainer>();
            var missionRuntimeService = missionContainer?.MissionRuntimeService;
            ServiceLocator.TryGetInstance(out KillChord.Runtime.Adaptor.InGame.StageSelect.SelectedBattleStageState selectedBattleStageState);

            System.Func<KillChord.Runtime.Domain.InGame.Mission.MissionActionKind?> targetActionProvider = () =>
            {
                if (selectedBattleStageState != null && selectedBattleStageState.HasSelectedBattleStage && selectedBattleStageState.CurrentStageDefinition.IsTutorial)
                {
                    if (missionRuntimeService != null && missionRuntimeService.MissionDefinition != null)
                    {
                        if (missionRuntimeService.MissionDefinition.ClearCondition is KillChord.Runtime.Domain.InGame.Mission.ClearCondition.ObjectiveSequenceClearCondition sequence)
                        {
                            var currentStepIndex = missionRuntimeService.MissionProgress.ObjectiveStepIndex;
                            var currentStep = sequence.GetStep(currentStepIndex);
                            if (currentStep != null && currentStep.Condition is KillChord.Runtime.Domain.InGame.Mission.ClearCondition.ActionRepeatCountClearCondition actionCondition)
                            {
                                return actionCondition.ActionKind;
                            }
                        }
                    }
                }
                return null;
            };

            // ガイド表示と判定は、音楽同期とターゲット状態の双方を参照するためPresenterへ集約する。
            RhythmGuidePresenter presenter = new RhythmGuidePresenter(
                musicSyncService,
                new RhythmGuideUsecase(_rhythmJudgmentDefinitionAsset.ToDefinition()),
                targetingSystem,
                targetActionProvider
            );

            new ACLikeRhythmGuideViewModel(_rhythmGuideView, presenter);

            if (_rhythmGuidePostEffectView == null || _effectConfig == null)
            {
                Debug.LogError($"[{nameof(ACLikeRhythmGuideInitializer)}] 全画面演出Viewまたは演出設定が未設定です。", this);
                return false;
            }

            PlayerAttackController playerAttackController =
                ServiceLocator.GetInstance<PlayerModuleContainer>()?.PlayerAttackController;

            if (playerAttackController == null)
            {
                Debug.LogError($"[{nameof(ACLikeRhythmGuideInitializer)}] {nameof(PlayerAttackController)} が見つかりません。PlayerInitializer が先に初期化されているか確認してください。", this);
                return false;
            }

            // 攻撃入力の購読とジャスト判定はPresenterが持ち、ViewModelは設定に基づく表示反映のみを担う。
            _postEffectPresenter = new RhythmGuidePostEffectPresenter(
                playerAttackController,
                _rhythmGuideView,
                new RhythmGuidePostEffectViewModel(_rhythmGuidePostEffectView, _effectConfig));

            return true;
        }

        /// <summary>
        ///     全画面Vignette用Presenterを破棄する。
        /// </summary>
        public override void Shutdown()
        {
            _postEffectPresenter?.Dispose();
            _postEffectPresenter = null;
            _isRegisteredToPlayDirector = false;
        }

        [Tooltip("リズム判定定義アセット。")]
        [SerializeField] private RhythmJudgmentDefinitionAsset _rhythmJudgmentDefinitionAsset;
        [Tooltip("リズムガイドView。")]
        [SerializeField] private ACLikeRhythmGuideView _rhythmGuideView;
        [Tooltip("リズムガイドのフルスクリーン演出View。")]
        [SerializeField] private RhythmGuidePostEffectView _rhythmGuidePostEffectView;
        [Tooltip("リズムガイドの演出設定。ACLikeRhythmGuideViewに設定した物と同じアセットを指定。")]
        [SerializeField] private ACLikeRhythmGuideEffectConfig _effectConfig;

        private bool _isRegisteredToPlayDirector;
        private RhythmGuidePostEffectPresenter _postEffectPresenter;

        /// <summary>
        ///     リズムガイドViewをゲームプレイ開始対象へ登録します。
        /// </summary>
        private void RegisterGameplayControllable()
        {
            // 再初期化で多重登録されないようにする。
            if (_isRegisteredToPlayDirector)
            {
                return;
            }

            InGamePlayDirector inGamePlayDirector = FindFirstObjectByType<InGamePlayDirector>();
            if (inGamePlayDirector == null)
            {
                Debug.LogError($"[{nameof(ACLikeRhythmGuideInitializer)}] {nameof(InGamePlayDirector)} が見つかりません。", this);
                return;
            }

            inGamePlayDirector.AddGamePlayControllable(_rhythmGuideView);
            _isRegisteredToPlayDirector = true;
        }
    }
}
