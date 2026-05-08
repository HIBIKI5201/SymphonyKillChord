using KillChord.Runtime.Adaptor.InGame.Camera.Target;
using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.InfraStructure.InGame.Music;
using KillChord.Runtime.View.InGame.Music;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Music
{
    /// <summary>
    ///     リズムガイド機能の初期化を行うクラス。
    /// </summary>
    public class RhythmGuideInitializer : MonoBehaviour
    {
        /// <summary>
        ///     リズムガイド機能を初期化する。
        /// </summary>
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

        [Tooltip("リズムガイド定義アセット。")]
        [SerializeField] private RhythmGuideDefinitionAsset _rhythmGuideDefinitionAsset;
        [Tooltip("リズムガイドView。")]
        [SerializeField] private RhythmGuideView _rhythmGuideView;
        [Tooltip("リズムガイド更新View。")]
        [SerializeField] private RhythmGuideUpdeteView _rhythmGuideUpdateView;
    }
}
