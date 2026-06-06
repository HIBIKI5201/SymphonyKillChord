using KillChord.Runtime.Adaptor.OutGame.Scenario;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition.OutGame.Scenario
{
    /// <summary>
    ///     シナリオ選択に必要な状態を初期化するクラス。
    /// </summary>
    public class OutGameScenarioInitializer : MonoBehaviour
    {
        private bool _registeredByThisInitializer;

        private void Awake()
        {
            if (ServiceLocator.TryGetInstance<SelectedScenarioState>(out _))
            {
                return;
            }

            ServiceLocator.RegisterInstance(new SelectedScenarioState());
            _registeredByThisInitializer = true;
        }

        private void OnDestroy()
        {
            if (_registeredByThisInitializer)
            {
                ServiceLocator.UnregisterInstance<SelectedScenarioState>();
            }
        }
    }
}
