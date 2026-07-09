using KillChord.Runtime.Adaptor.InGame.Camera.Target;
using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.InfraStructure.InGame.Music;
using KillChord.Runtime.View.InGame.Music;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition
{
    public class ACLikeRhythmGuideInitializer : MonoBehaviour
    { /// <summary>
      ///     リズムガイド機能を初期化する。
      /// </summary>
        public void Initialize()
        {
            Debug.Assert(_rhythmGuideDefinitionAsset != null, "RhythmGuideDefinitionAsset の参照が未設定です。RhythmGuideDefinitionAsset を設定してください。");
            Debug.Assert(_rhythmGuideView != null, "RhythmGuideView の参照が未設定です。RhythmGuideView を設定してください。");

            IMusicSyncService musicSyncService =
                ServiceLocator.GetInstance<IMusicSyncService>();

            if (musicSyncService == null)
            {
                Debug.LogError($"{nameof(IMusicSyncService)} が見つかりません。MusicSyncInitializer が先に初期化されているか確認してください。");
                return;
            }

            TargetSelectorController targetSelectorController =
                ServiceLocator.GetInstance<TargetSelectorController>();

            if (targetSelectorController == null)
            {
                Debug.LogError($"{nameof(TargetSelectorController)} が見つかりません。TargetSelectorController が登録されているか確認してください。");
                return;
            }

            RhythmGuideUsecase usecase = new RhythmGuideUsecase(_rhythmGuideDefinitionAsset.ToDefinition());

            RhythmGuidePresenter presenter = new RhythmGuidePresenter(
                musicSyncService,
                usecase,
                targetSelectorController
            );

            ACLikeRhythmGuideViewModel viewModel = new ACLikeRhythmGuideViewModel(_rhythmGuideView, presenter);
        }

        [Tooltip("リズムガイド定義アセット。")]
        [SerializeField] private RhythmGuideDefinitionAsset _rhythmGuideDefinitionAsset;
        [Tooltip("リズムガイドView。")]
        [SerializeField] private ACLikeRhythmGuideView _rhythmGuideView;
    }
}
