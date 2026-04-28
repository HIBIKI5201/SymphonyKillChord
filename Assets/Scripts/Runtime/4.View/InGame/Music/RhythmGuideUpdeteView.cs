using KillChord.Runtime.Adaptor.InGame.Music;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Music
{
    public class RhythmGuideUpdeteView : MonoBehaviour
    {
        public void Initialize(RhythmGuideView view, RhythmGuidePresenter presenter, RhythmGuideViewModel viewModel)
        {
            _view = view;
            _presenter = presenter;
            _viewModel = viewModel;
        }

        private RhythmGuideViewModel _viewModel;
        private RhythmGuideView _view;
        private RhythmGuidePresenter _presenter;

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
