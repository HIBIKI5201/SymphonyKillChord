using KillChord.Runtime.Adaptor.InGame.Music;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Music
{
    public sealed class NecrodancerRhythmGuideViewModel
    {
        public NecrodancerRhythmGuideViewModel(NecrodancerRhythmGuideView view)
        {
            _view = view;



            view.OnUpdate += OnUpdate;
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
        }

        private bool _isPlaying;

        private readonly NecrodancerRhythmGuideView _view;
        private readonly RhythmGuidePresenter _presenter;
    }
}
