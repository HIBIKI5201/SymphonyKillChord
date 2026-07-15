using KillChord.Runtime.Adaptor.OutGame.Scenario;
using KillChord.Runtime.Composition.OutGame.Bootstrap;
using SymphonyFrameWork.System.ServiceLocate;

namespace KillChord.Runtime.Composition.OutGame.Scenario
{
    /// <summary>
    ///     シナリオ選択に必要な状態を初期化するクラス。
    /// </summary>
    public sealed class OutGameScenarioInitializer : OutGameInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(OutGameScenarioInitializer);

        /// <summary> 実行順です。 </summary>
        public override int Order => 10;

        /// <summary>
        ///     シナリオ選択状態を登録します。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Build()
        {
            if (ServiceLocator.TryGetInstance<SelectedScenarioState>(out _))
            {
                return true;
            }

            _registeredByThisInitializer = ServiceLocator.RegisterInstance(new SelectedScenarioState());
            return _registeredByThisInitializer;
        }

        /// <summary>
        ///     登録したシナリオ選択状態を解除します。
        /// </summary>
        public override void Shutdown()
        {
            if (_registeredByThisInitializer)
            {
                ServiceLocator.UnregisterInstance<SelectedScenarioState>();
                _registeredByThisInitializer = false;
            }
        }

        private bool _registeredByThisInitializer;
    }
}
