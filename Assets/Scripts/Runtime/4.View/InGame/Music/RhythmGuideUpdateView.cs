using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.View.InGame.Sequence;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Music
{
    /// <summary>
    ///     リズムガイドの更新を毎フレーム行うViewクラス。
    /// </summary>
    public class RhythmGuideUpdateView : MonoBehaviour, IGameplayControllable
    {
        /// <summary>
        ///     更新用Viewを初期化する。
        /// </summary>
        /// <param name="view"> リズムガイドView。 </param>
        /// <param name="presenter"> リズムガイドプレゼンター。 </param>
        /// <param name="viewModel"> リズムガイドビューモデル。 </param>
        public void Initialize(RhythmGuideView view, RhythmGuidePresenter presenter, RhythmGuideViewModel viewModel)
        {
            _view = view;
            _presenter = presenter;
            _viewModel = viewModel;
        }
        /// <summary>
        ///    ゲームプレイの開始処理を行います。
        /// </summary>
        public void StartGameplay()
        {
            _isPlaying = true;
        }
        /// <summary>
        ///    ゲームプレイの停止処理を行います。
        /// </summary>
        public void StopGameplay()
        {
            _isPlaying = false;
        }

        private RhythmGuideViewModel _viewModel;
        private RhythmGuideView _view;
        private RhythmGuidePresenter _presenter;
        private bool _isPlaying;

        /// <summary>
        ///     毎フレームの更新処理。プレゼンターからデータを受け取り、ビューモデルとViewを更新する。
        /// </summary>
        private void Update()
        {
            if (!_isPlaying)
            {
                return;
            }

            if (_presenter == null || _viewModel == null || _view == null)
            {
                return;
            }

            RhythmGuideDto dto = _presenter.CreateDto();
            _viewModel.Apply(dto);
            _view.Render(_viewModel);
        }
    }
}
