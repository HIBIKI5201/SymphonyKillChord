using KillChord.Runtime.Adaptor.InGame.Target;
using KillChord.Runtime.View.InGame.Target;
using SymphonyFrameWork.System.ServiceLocate;
using System;

namespace KillChord.Runtime.Composition.InGame.Target
{
    /// <summary>
    ///     ターゲットシステムの依存解決とサービス登録を行う初期化クラス。
    /// </summary>
    public sealed class TargetSystemInitializer : IDisposable
    {
        /// <summary> ターゲット選択ViewModel。 </summary>
        public ITargetSystemViewModel TargetingSystemViewModel { get; private set; }

        /// <summary> ターゲットシステムのコントローラー。 </summary>
        public TargetSystemController TargetSystemController { get; private set; }

        /// <summary> ターゲットEntityレジストリです。 </summary>
        public TargetEntityRegistry TargetEntityRegistry { get; private set; }

        /// <summary> 扇形範囲クエリです。 </summary>
        public TargetAreaQuery TargetAreaQuery { get; private set; }

        /// <summary>
        ///     ターゲットシステムを初期化する。
        /// </summary>
        public void Initialize()
        {
            TargetEntityRegistry = new TargetEntityRegistry();
            TargetingSystemViewModel = new TargetingSystem();
            TargetSystemController = new TargetSystemController(TargetingSystemViewModel, TargetEntityRegistry);
            TargetAreaQuery = new TargetAreaQuery(TargetingSystemViewModel, TargetEntityRegistry);
            ServiceLocator.RegisterInstance(TargetingSystemViewModel);
            ServiceLocator.RegisterInstance(TargetSystemController);
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

            ServiceLocator.UnregisterInstance(TargetSystemController);
            ServiceLocator.UnregisterInstance(TargetingSystemViewModel);
            TargetSystemController = null;
            TargetingSystemViewModel = null;
            TargetEntityRegistry = null;
            TargetAreaQuery = null;
            _isRegistered = false;
        }

        private bool _isRegistered;
    }
}
