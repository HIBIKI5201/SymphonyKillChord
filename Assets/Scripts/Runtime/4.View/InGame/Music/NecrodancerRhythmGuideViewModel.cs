using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.View.InGame.Sequence;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Music
{
    public sealed class NecrodancerRhythmGuideViewModel : IGameplayControllable
    {
        public NecrodancerRhythmGuideViewModel(NecrodancerRhythmGuideView view, RhythmGuidePresenter presenter)
        {
            _view = view;
            _presenter = presenter;

            view.OnUpdate += OnUpdate;
            view.OnStartGameplay += StartGameplay;
            view.OnStopGameplay += StopGameplay;
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
        private void OnUpdate()
        {
            if (!_isPlaying)
                return;

            RhythmGuideDto dto = _presenter.CreateDto(Time.unscaledTime);

            _view.SetAlpha(dto.HasTarget);
            _view.SetBeatsOffset(dto.IndicatorNormalized);
        }

        private bool _isPlaying;

        private readonly NecrodancerRhythmGuideView _view;
        private readonly RhythmGuidePresenter _presenter;
    }
}
