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
        /// <param name="healthHudViewModel">バインドするHP HUDのViewModel</param>
        public void InitializePlayerHpHud(IHealthHudViewModel healthHudViewModel)
        {
            _playerHealthHudView.Bind(healthHudViewModel);

            // テキストのみのHP表示は任意配置のため、参照が無い場合はスキップする
            if (_playerHealthTextView != null)
            {
                _playerHealthTextView.Bind(healthHudViewModel);
            }
        }
        public override bool Build()
        {
            ServiceLocator.RegisterInstance<InGameHudInitializer>(this, LocateTypeEnum.Locator);
            return true;
        }
        public override void Shutdown()
        {
            ServiceLocator.UnregisterInstance<InGameHudInitializer>(this);
        }

        [SerializeField, Tooltip("プレイヤーのHPバーHUDのView")] private HealthHudView _playerHealthHudView;
        [SerializeField, Tooltip("プレイヤーのHPテキスト表示のView")] private HealthTextView _playerHealthTextView;
    }
}
