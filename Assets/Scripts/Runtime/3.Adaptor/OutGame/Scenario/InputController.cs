using KillChord.Runtime.Application;

namespace KillChord.Runtime.Adaptor
{
    public class InputController
    {
        public InputController(ScenarioAdvanceGate gate, IScenarioPlaybackControl playbackControl)
        {
            _gate = gate;
            _playbackControl = playbackControl;
        }

        public void MouseClick()
        {
            _gate.NotifyNext();
        }

        public void SetFastForward(bool enabled)
        {
            _playbackControl.SetFastForward(enabled);
        }

        public void TogglePause()
        {
            _playbackControl.TogglePause();
        }

        public void Skip()
        {
            _playbackControl.RequestSkip();
        }

        private readonly ScenarioAdvanceGate _gate;
        private readonly IScenarioPlaybackControl _playbackControl;
    }
}
