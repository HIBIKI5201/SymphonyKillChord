using KillChord.Runtime.Adaptor.InGame.Mission;
using KillChord.Runtime.Adaptor.Persistent.Input;
using KillChord.Runtime.View.Persistent.Input;
using System;

namespace KillChord.Runtime.Composition.InGame.Mission
{
    /// <summary>
    ///     シナリオ再生中のInputActionMapを切り替えるコントローラー。
    /// </summary>
    public sealed class InGameScenarioInputModeController : IScenarioInputModeController
    {
        public InGameScenarioInputModeController(UnityInputMapController inputMapController)
        {
            _inputMapController = inputMapController ?? throw new ArgumentNullException(nameof(inputMapController));
        }

        /// <inheritdoc />
        public void EnterScenarioInputMode()
        {
            _inputMapController.EnableCommonWith(InputMapNames.Scenario);
        }

        /// <inheritdoc />
        public void ExitScenarioInputMode()
        {
            _inputMapController.EnableCommonWith(InputMapNames.InGame);
        }

        private readonly UnityInputMapController _inputMapController;
    }
}
