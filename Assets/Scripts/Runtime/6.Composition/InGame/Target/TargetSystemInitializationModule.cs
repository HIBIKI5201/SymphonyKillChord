using KillChord.Runtime.Composition.InGame.Bootstrap;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Target
{
    /// <summary>
    ///     ターゲットシステムを初期化して公開するモジュールです。
    /// </summary>
    public sealed class TargetSystemInitializationModule : InGameInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(TargetSystemInitializationModule);

        /// <summary> 実行順です。 </summary>
        public override int Order => 100;

        /// <summary>
        ///     ターゲットシステムを構築して公開します。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Build()
        {
            _initializer = new TargetSystemInitializer();
            _initializer.Initialize();
            _container = new TargetSystemModuleContainer(
                _initializer.TargetSystemController,
                _initializer.TargetingSystemViewModel);
            ServiceLocator.RegisterInstance(_container);
            _isRegistered = true;
            return _container.TargetSystemController != null
                && _container.TargetSystemViewModel != null;
        }

        /// <summary>
        ///     登録済みサービスを解除します。
        /// </summary>
        public override void Shutdown()
        {
            if (!_isRegistered)
            {
                return;
            }

            ServiceLocator.UnregisterInstance<TargetSystemModuleContainer>();
            _initializer?.Dispose();
            _initializer = null;
            _container = null;
            _isRegistered = false;
        }

        private TargetSystemInitializer _initializer;
        private TargetSystemModuleContainer _container;
        private bool _isRegistered;
    }
}
