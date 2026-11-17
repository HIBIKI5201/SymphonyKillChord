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
        /// <param name="view"> バーのリズムガイドのViewです。 </param>
        /// <param name="presenter"> リズムガイドの表示内容を作るPresenterです。 </param>
        /// <param name="ringView"> 試遊で見比べるリング型のViewです。使わない場合はnull。 </param>
        public ACLikeRhythmGuideViewModel(ACLikeRhythmGuideView view, RhythmGuidePresenter presenter,
            RhythmGuideRingView ringView = null)
        {
            this._view = view;
            this._presenter = presenter;
            this._ringView = ringView;

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
            _view.SetLockedRange(dto.LockedBarProgress);
            _view.ConfigureZones(dto.Zones, dto.GuideLengthInBars);
            _view.SetAlpha(dto.HasTarget);
            _view.SetBeatsOffset(dto.IndicatorNormalized, dto.IsJustTiming, dto.CurrentBeatCount);

            // リング型の案では、同じ表示内容をリングにも渡す。
            if (_ringView == null)
            {
                return;
            }

            _ringView.SetTargetBeatCount(dto.TargetBeatCount);
            _ringView.SetLockedRange(dto.LockedBarProgress);
            _ringView.ConfigureZones(dto.Zones, dto.GuideLengthInBars);
            _ringView.SetAlpha(dto.HasTarget);
            _ringView.SetBeatsOffset(dto.IndicatorNormalized, dto.IsJustTiming);
        }

        private bool _isPlaying;
        private bool _isDisposed;

        private readonly ACLikeRhythmGuideView _view;
        private readonly RhythmGuidePresenter _presenter;
        private readonly RhythmGuideRingView _ringView;
    }
}
