using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.View.InGame.Sequence;
using System;

namespace KillChord.Runtime.View.InGame.Music
{
    /// <summary>
    ///     リズムガイドのビューを毎フレーム更新する ViewModel。
    /// </summary>
    public sealed class ACLikeRhythmGuideViewModel : IGameplayControllable, IDisposable
    {
        /// <summary>
        ///     ビューとプレゼンターを指定して生成し、ビューの更新イベントを購読する。
        /// </summary>
        public ACLikeRhythmGuideViewModel(ACLikeRhythmGuideView view, RhythmGuidePresenter presenter)
        {
            this._view = view;
            this._presenter = presenter;

            view.OnUpdate += Update;
            view.OnStartGameplay += StartGameplay;
            view.OnStopGameplay += StopGameplay;
        }

        /// <summary>
        ///     ゲームプレイ開始時にガイドの更新を始める。
        /// </summary>
        public void StartGameplay()
        {
            _isPlaying = true;
        }

        /// <summary>
        ///     ゲームプレイ停止時にガイドの更新を止める。
        /// </summary>
        public void StopGameplay()
        {
            _isPlaying = false;
        }

        /// <summary>
        ///     ビューのイベントの購読を解除する。
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            _isPlaying = false;
            if (_view == null)
            {
                return;
            }

            _view.OnUpdate -= Update;
            _view.OnStartGameplay -= StartGameplay;
            _view.OnStopGameplay -= StopGameplay;
        }

        /// <summary>
        ///     再生中であれば、リズムガイドの表示を更新する。
        /// </summary>
        private void Update()
        {
            if (!_isPlaying)
            {
                return;
            }


            RhythmGuideDto dto = _presenter.CreateDto();

            _view.SetTargetBeatCount(dto.TargetBeatCount);
            _view.ConfigureZones(dto.Zones, dto.GuideLengthInBars);
            _view.SetAlpha(dto.HasTarget);
            _view.SetBeatsOffset(dto.IndicatorNormalized, dto.IsJustTiming, dto.CurrentBeatCount);
        }

        private bool _isPlaying;
        private bool _isDisposed;

        private readonly ACLikeRhythmGuideView _view;
        private readonly RhythmGuidePresenter _presenter;
    }
}
