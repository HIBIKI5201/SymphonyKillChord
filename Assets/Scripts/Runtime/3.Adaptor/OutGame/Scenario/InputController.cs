using UnityEngine;

namespace KillChord.Runtime.Adaptor
{
    public class InputController
    {
        //テスト用入力システム

        public InputController(ScenarioAdvanceGate gate)
        {
            _gate = gate;
        }

        public void MouseClick()
        {
            _gate.NotifyNext();
        }

        private readonly ScenarioAdvanceGate _gate;
    }
}
