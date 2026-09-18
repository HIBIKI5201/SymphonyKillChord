using KillChord.Runtime.Application.OutGame.Scenario;

namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// 入力操作を再生制御と送り待ち解除へ変換する。
    /// </summary>
    public class ScenarioInputController
    {
        /// <summary>
        /// 入力操作を再生制御へ変換する依存関係を受け取る。
        /// </summary>
        public ScenarioInputController(
            ScenarioAdvanceGate gate,
            TextEventHandler textEventHandler,
            IScenarioPlaybackControl playbackControl,
            IScenarioPlaybackState state)
        {
            _gate = gate;
            _textEventHandler = textEventHandler;
            _playbackControl = playbackControl;
            _state = state;
        }

        /// <summary> スキップ確認中かを示す。 </summary>
        public bool IsSkipConfirmationOpen { get; private set; }

        /// <summary> 自動送りが有効かを示す。 </summary>
        public bool IsAutoAdvance => _state.IsAutoAdvance;

        /// <summary>
        /// クリック入力を次送り操作として通知する。
        /// </summary>
        public void MouseClick()
        {
            if (IsSkipConfirmationOpen || !_state.IsPlaying) { return; }

            if (_textEventHandler.TryCompleteCurrentText())
            {
                return;
            }

            _gate.NotifyNext();
        }

        /// <summary>
        /// 早送り状態を切り替える。
        /// </summary>
        public void SetFastForward(bool enabled)
        {
            if (enabled && IsSkipConfirmationOpen) { return; }
            _playbackControl.SetFastForward(enabled);
        }

        /// <summary>
        /// 自動送り状態を切り替える。
        /// </summary>
        public void ToggleAutoAdvance()
        {
            if (IsSkipConfirmationOpen) { return; }
            bool wasAutoAdvance = _state.IsAutoAdvance;

            _playbackControl.ToggleAutoAdvance();

            if (!wasAutoAdvance && _state.IsAutoAdvance)
            {
                // 手動送り待機中にAutoへ切り替えた場合、現在の待機を解除する。
                _gate.NotifyNext();
            }
        }

        /// <summary>
        /// 一時停止状態を切り替える。
        /// </summary>
        public void TogglePause()
        {
            if (IsSkipConfirmationOpen) { return; }
            _playbackControl.TogglePause();
        }

        /// <summary>
        ///     再生中のシナリオを一時停止し、スキップ確認を開始する。
        /// </summary>
        public bool BeginSkipConfirmation()
        {
            if (IsSkipConfirmationOpen || !_state.IsPlaying) { return false; }
            _wasPausedBeforeConfirmation = _state.IsPaused;
            IsSkipConfirmationOpen = true;
            if (!_state.IsPaused) { _playbackControl.TogglePause(); }
            return true;
        }

        /// <summary>
        ///     確認を閉じ、確定されたスキップを再生制御へ通知する。
        /// </summary>
        public void ConfirmSkip()
        {
            if (!IsSkipConfirmationOpen) { return; }
            _playbackControl.RequestSkip();
            CancelSkipConfirmation();
        }

        /// <summary>
        ///     確認を解除し、開始前の一時停止状態を復元する。
        /// </summary>
        public void CancelSkipConfirmation()
        {
            if (!IsSkipConfirmationOpen) { return; }
            IsSkipConfirmationOpen = false;
            if (_state.IsPaused != _wasPausedBeforeConfirmation)
            {
                _playbackControl.TogglePause();
            }
        }

        private readonly ScenarioAdvanceGate _gate;
        private readonly TextEventHandler _textEventHandler;
        private readonly IScenarioPlaybackControl _playbackControl;
        private readonly IScenarioPlaybackState _state;
        private bool _wasPausedBeforeConfirmation;
    }
}
