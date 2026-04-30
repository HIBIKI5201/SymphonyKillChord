using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.InfraStructure.InGame.Music;
using KillChord.Runtime.View.InGame.Music;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Music
{
    public class RhythmGuideInitializer : MonoBehaviour
    {
        public void Initialize()
        {
            if (_rhythmGuideDefinitionAsset == null || _rhythmGuideView == null || _rhythmGuideUpdateView == null)
            {
                Debug.LogError("RhythmGuideInitializer の参照が未設定です。RhythmGuideDefinitionAsset / RhythmGuideView / RhythmGuideUpdeteView を設定してください。");
                return;
            }

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

            RhythmGuideDefinition definition = _rhythmGuideDefinitionAsset.ToDefinition();
            RhythmGuideUsecase usecase = new RhythmGuideUsecase(definition);

            RhythmGuidePresenter presenter = new RhythmGuidePresenter(
                musicSyncService,
                usecase,
                targetSelectorController
            );

            RhythmGuideViewModel viewModel = new RhythmGuideViewModel();

            _rhythmGuideUpdateView.Initialize(
                _rhythmGuideView,
                presenter,
                viewModel
            );
        }

        [SerializeField] private RhythmGuideDefinitionAsset _rhythmGuideDefinitionAsset;
        [SerializeField] private RhythmGuideView _rhythmGuideView;
        [SerializeField] private RhythmGuideUpdeteView _rhythmGuideUpdateView;
    }
}
