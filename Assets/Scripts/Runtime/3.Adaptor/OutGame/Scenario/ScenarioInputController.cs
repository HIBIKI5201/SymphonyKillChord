using KillChord.Runtime.Application.OutGame.Scenario;
using static UnityEngine.ParticleSystem;

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
        public ScenarioInputController(ScenarioAdvanceGate gate, IScenarioPlaybackControl playbackControl, IScenarioPlaybackState state)
        {
            _gate = gate;
            _playbackControl = playbackControl;
            _state = state;
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
        /// 自動送り状態を切り替える。
        /// </summary>
        public void ToggleAutoAdvance()
        {
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
        private readonly IScenarioPlaybackState _state;
    }
}