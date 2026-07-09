using SymphonyFrameWork.System.ServiceLocate;
using System;
using KillChord.Runtime.View.InGame.Target;

namespace KillChord.Runtime.Composition.InGame.Target
{
    /// <summary>
    ///     ターゲットシステムの依存解決とサービス登録を行う初期化クラス。
    /// </summary>
    public sealed class TargetSystemInitializer : IDisposable
    {
        /// <summary> ターゲットシステム本体。 </summary>
        public TargetingSystem TargetingSystem { get; private set; }

        /// <summary>
        ///     ターゲットシステムを初期化する。
        /// </summary>
        public void Initialize()
        {
            TargetingSystem = new TargetingSystem();
            ServiceLocator.RegisterInstance(TargetingSystem);
            _isRegistered = true;
        }

        /// <summary>
        ///     登録済みサービスを解除する。
        /// </summary>
        public void Dispose()
        {
            if (!_isRegistered)
            {
                return;
            }

            ServiceLocator.UnregisterInstance(TargetingSystem);
            TargetingSystem = null;
            _isRegistered = false;
        }

        private bool _isRegistered;
    }
}
