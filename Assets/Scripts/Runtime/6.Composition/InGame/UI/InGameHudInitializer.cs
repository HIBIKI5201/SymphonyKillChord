using KillChord.Runtime.Adaptor.InGame.UI;
using KillChord.Runtime.Composition.InGame.Bootstrap;
using KillChord.Runtime.View.InGame.UI;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.UI
{
    /// <summary>
    ///     InGameのHUDを初期化するクラス。
    /// </summary>
    public sealed class InGameHudInitializer : InGameInitializationModuleBase
    {
        public override string ModuleName => nameof(InGameHudInitializer);

        public override int Order => 495;
        /// <summary>
        ///     HPバーHUDの初期化。
        /// </summary>
        /// <param name="entity">表示対象のEntity</param>
        /// <param name="healthHudView">表示対象のHP HUD View</param>
        public void InitializePlayerHpHud(IHealthHudViewModel healthHudViewModel)
        {
            _playerHealthHudView.Bind(healthHudViewModel);
        }
        public override bool Build()
        {
            ServiceLocator.RegisterInstance<InGameHudInitializer>(this, LocateType.Locator);
            return true;
        }
        public override void Shutdown()
        {
            ServiceLocator.UnregisterInstance<InGameHudInitializer>(this);
        }

        [SerializeField] private HealthHudView _playerHealthHudView;
    }
}
