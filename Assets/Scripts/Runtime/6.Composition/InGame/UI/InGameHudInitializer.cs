using KillChord.Runtime.Adaptor.InGame.UI;
using KillChord.Runtime.View.InGame.UI;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.UI
{
    /// <summary>
    ///     InGameのHUDを初期化するクラス。
    /// </summary>
    public class InGameHudInitializer : MonoBehaviour
    {
        /// <summary>
        ///     HPバーHUDの初期化。
        /// </summary>
        /// <param name="entity">表示対象のEntity</param>
        /// <param name="healthHudView">表示対象のHP HUD View</param>
        public void InitializePlayerHpHud(IHealthHudViewModel healthHudViewModel)
        {
            _playerHealthHudView.Bind(healthHudViewModel);
        }

        [SerializeField] private HealthHudView _playerHealthHudView;

        private void Awake()
        {
            ServiceLocator.RegisterInstance<InGameHudInitializer>(this, LocateTypeEnum.Locator);
        }

        private void OnDestroy()
        {
            ServiceLocator.UnregisterInstance<InGameHudInitializer>(this);
        }
    }
}
