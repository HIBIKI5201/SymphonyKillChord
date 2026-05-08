using KillChord.Runtime.Adaptor.InGame.Music;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Music
{
    /// <summary>
    ///     リズムガイドの更新を毎フレーム行うViewクラス。
    /// </summary>
    public class RhythmGuideUpdateView : MonoBehaviour
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

        private RhythmGuideViewModel _viewModel;
        private RhythmGuideView _view;
        private RhythmGuidePresenter _presenter;

        /// <summary>
        ///     毎フレームの更新処理。プレゼンターからデータを受け取り、ビューモデルとViewを更新する。
        /// </summary>
        private void Update()
        {
            if (_presenter == null || _viewModel == null || _view == null)
            {
                return;
            }

            RhythmGuideDto dto = _presenter.CreateDto(Time.unscaledTime);
            _viewModel.Apply(dto);
            _view.Render(_viewModel);
        }
    }
}
