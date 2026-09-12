using KillChord.Runtime.Adaptor.InGame.Target;
using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.Application.InGame.Music;
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
            if (_rhythmGuideView == null || _rhythmGuideUpdateView == null)
            {
                Debug.LogError($"[{nameof(RhythmGuideInitializer)}] RhythmGuideView / RhythmGuideUpdateView の参照を設定してください。", this);
                return;
            }

            IMusicSyncService musicSyncService =
                ServiceLocator.GetInstance<IMusicSyncService>();

            if (musicSyncService == null)
            {
                Debug.LogError($"{nameof(IMusicSyncService)} が見つかりません。MusicSyncInitializer が先に初期化されているか確認してください。");
                return;
            }

            TargetSystemController targetingSystem =
                ServiceLocator.GetInstance<TargetSystemController>();

            if (targetingSystem == null)
            {
                Debug.LogError($"{nameof(TargetSystemController)} が見つかりません。TargetSystemController が登録されているか確認してください。");
                return;
            }

            RhythmGuideUsecase usecase = new RhythmGuideUsecase();

            RhythmGuidePresenter presenter = new RhythmGuidePresenter(
                musicSyncService,
                usecase,
                targetingSystem
            );

            RhythmGuideViewModel viewModel = new RhythmGuideViewModel();

            _rhythmGuideUpdateView.Initialize(
                _rhythmGuideView,
                presenter,
                viewModel
            );
        }

        [Tooltip("リズムガイドView。")]
        [SerializeField] private RhythmGuideView _rhythmGuideView;
        [Tooltip("リズムガイド更新View。")]
        [SerializeField] private RhythmGuideUpdateView _rhythmGuideUpdateView;
    }
}
