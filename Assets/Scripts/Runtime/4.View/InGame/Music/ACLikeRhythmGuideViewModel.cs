using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.View.InGame.Sequence;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Music
{
    public sealed class ACLikeRhythmGuideViewModel : IGameplayControllable
    {
        public ACLikeRhythmGuideViewModel(ACLikeRhythmGuideView view, RhythmGuidePresenter presenter)
        {
            this._view = view;
            this._presenter = presenter;

            view.OnUpdate += Update;
            view.OnStartGameplay += StartGameplay;
            view.OnStopGameplay += StopGameplay;
        }

        public void StartGameplay()
        {
            _isPlaying = true;
        }

        public void StopGameplay()
        {
            _isPlaying = false;
        }

        private void Update()
        {
            if (!_isPlaying)
                return;


            RhythmGuideDto dto = _presenter.CreateDto(Time.unscaledTime);

            _view.SetAlpha(dto.HasTarget);
            _view.SetBeatsOffset(dto.IndicatorNormalized);
        }

        private bool _isPlaying;

        private readonly ACLikeRhythmGuideView _view;
        private readonly RhythmGuidePresenter _presenter;
    }
}
