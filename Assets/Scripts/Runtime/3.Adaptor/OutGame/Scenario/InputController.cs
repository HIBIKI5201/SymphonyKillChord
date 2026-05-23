using KillChord.Runtime.Application.OutGame.Scenario;

namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// 入力操作を再生制御と送り待ち解除へ変換する。
    /// </summary>
    public class InputController
    {
        /// <summary>
        /// 入力操作を再生制御へ変換する依存関係を受け取る。
        /// </summary>
        public InputController(ScenarioAdvanceGate gate, IScenarioPlaybackControl playbackControl)
        {
            _gate = gate;
            _playbackControl = playbackControl;
        }

        /// <summary>
        /// クリック入力を次送り操作として通知する。
        /// </summary>
        public void MouseClick()
        {
            _gate.NotifyNext();
        }

        /// <summary>
        /// 早送り状態を切り替える。
        /// </summary>
        public void SetFastForward(bool enabled)
        {
            _playbackControl.SetFastForward(enabled);
        }

        /// <summary>
        /// 一時停止状態を切り替える。
        /// </summary>
        public void TogglePause()
        {
            _playbackControl.TogglePause();
        }

        /// <summary>
        /// スキップ操作を再生制御へ通知する。
        /// </summary>
        public void Skip()
        {
            _playbackControl.RequestSkip();
        }

        private readonly ScenarioAdvanceGate _gate;
        private readonly IScenarioPlaybackControl _playbackControl;
    }
}