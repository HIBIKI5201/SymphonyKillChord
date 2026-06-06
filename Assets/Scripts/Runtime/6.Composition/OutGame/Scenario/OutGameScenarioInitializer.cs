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
        private void Awake()
        {
            if (ServiceLocator.TryGetInstance<SelectedScenarioState>(out _))
            {
                return;
            }

            ServiceLocator.RegisterInstance(new SelectedScenarioState());
        }

        private void OnDestroy()
        {
            ServiceLocator.UnregisterInstance<SelectedScenarioState>();
        }
    }
}
