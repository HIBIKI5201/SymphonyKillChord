using KillChord.Runtime.Adaptor.InGame.Target;
using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Composition.InGame.Bootstrap;
using KillChord.Runtime.Composition.InGame.Music;
using KillChord.Runtime.Composition.InGame.Target;
using KillChord.Runtime.InfraStructure.InGame.Music;
using KillChord.Runtime.View.InGame.Music;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition
{
    /// <summary>
    ///     AC風リズムガイドを初期化するモジュールです。
    /// </summary>
    public class ACLikeRhythmGuideInitializer : InGameInitializationModuleBase
    { /// <summary>
      ///     リズムガイド機能を初期化する。
      /// </summary>
        public override string ModuleName => nameof(ACLikeRhythmGuideInitializer);

        /// <summary> 実行順です。 </summary>
        public override int Order => 800;

        /// <summary>
        ///     他モジュールへ結合してリズムガイドを初期化する。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Ready()
        {
            if (_rhythmGuideDefinitionAsset == null || _rhythmGuideView == null)
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

            Initialize();
            return true;
        }

        /// <summary>
        ///     リズムガイド機能を初期化する。
        /// </summary>
        public void Initialize()
        {
            Debug.Assert(_rhythmGuideDefinitionAsset != null, "RhythmGuideDefinitionAsset の参照が未設定です。RhythmGuideDefinitionAsset を設定してください。");
            Debug.Assert(_rhythmGuideView != null, "RhythmGuideView の参照が未設定です。RhythmGuideView を設定してください。");

            MusicSyncModuleContainer musicSyncModuleContainer = ServiceLocator.GetInstance<MusicSyncModuleContainer>();
            IMusicSyncService musicSyncService = musicSyncModuleContainer?.MusicSyncService;

            if (musicSyncService == null)
            {
                Debug.LogError($"{nameof(IMusicSyncService)} が見つかりません。MusicSyncInitializer が先に初期化されているか確認してください。");
                return;
            }

            TargetSystemModuleContainer targetSystemModuleContainer = ServiceLocator.GetInstance<TargetSystemModuleContainer>();
            TargetSystemController targetingSystem = targetSystemModuleContainer?.TargetSystemController;

            if (targetingSystem == null)
            {
                Debug.LogError($"{nameof(TargetSystemController)} が見つかりません。TargetSystemController が登録されているか確認してください。");
                return;
            }

            RhythmGuideUsecase usecase = new RhythmGuideUsecase(_rhythmGuideDefinitionAsset.ToDefinition());

            RhythmGuidePresenter presenter = new RhythmGuidePresenter(
                musicSyncService,
                usecase,
                targetingSystem
            );

            ACLikeRhythmGuideViewModel viewModel = new ACLikeRhythmGuideViewModel(_rhythmGuideView, presenter);
        }

        [Tooltip("リズムガイド定義アセット。")]
        [SerializeField] private RhythmGuideDefinitionAsset _rhythmGuideDefinitionAsset;
        [Tooltip("リズムガイドView。")]
        [SerializeField] private ACLikeRhythmGuideView _rhythmGuideView;
    }
}
