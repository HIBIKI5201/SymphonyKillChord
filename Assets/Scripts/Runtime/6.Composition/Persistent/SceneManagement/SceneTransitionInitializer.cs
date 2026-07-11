using KillChord.Runtime.Adaptor.Persistent.Load;
using KillChord.Runtime.Adaptor.Persistent.SceneManagement;
using KillChord.Runtime.Application.Persistent.Load;
using KillChord.Runtime.Application.Persistent.SceneManagement;
using KillChord.Runtime.Composition.Persistent.Bootstrap;
using KillChord.Runtime.InfraStructure.Persistent.SceneManagement;
using KillChord.Runtime.View.Persistent.Load;
using KillChord.Runtime.View.Persistent.SceneManagement;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition.Persistent.SceneManagement
{
    /// <summary>
    ///     シーン遷移機能の初期化を行うクラス。
    /// </summary>
    public sealed class SceneTransitionInitializer : PersistentInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(SceneTransitionInitializer);

        /// <summary> 実行順です。 </summary>
        public override int Order => 0;

        [SerializeField, Tooltip("シーン遷移中に表示するロード画面")]
        private LoadingScreenView _loadingScreenView;

        [SerializeField, Tooltip("シーン遷移確認用のデバッグView")]
        private SceneTransitionView _debugView;

        /// <summary>
        ///     シーン遷移システムを構築して登録する。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Build()
        {
            // 既に SceneTransitionController が存在する場合はそれを使用し、存在しない場合は新たに作成して登録する。
            if (ServiceLocator.TryGetInstance<SceneTransitionController>(out var existingController)
                && ServiceLocator.TryGetInstance(out LoadingScreenController existingLoadingScreenController))
            {
                if (_loadingScreenView != null)
                {
                    _loadingScreenView.Initialize(
                        existingLoadingScreenController);
                }

                InitializeViews(existingController);
                return true;
            }

            _loadingScreenController = new LoadingScreenController();

            _loadingOperationExecutor = new LoadingOperationExecutor(_loadingScreenController);
            _sceneTransitionService = new SceneTransitionService();
            _sceneTransitionUsecase = new SceneTransitionUsecase(_sceneTransitionService, _loadingOperationExecutor);
            _sceneTransitionController = new SceneTransitionController(_sceneTransitionUsecase);

            ServiceLocator.RegisterInstance(_loadingScreenController);
            ServiceLocator.RegisterInstance<ILoadingSessionFactory>(_loadingScreenController);
            ServiceLocator.RegisterInstance<ILoadingOperationExecutor>(_loadingOperationExecutor);
            ServiceLocator.RegisterInstance<ISceneTransitionService>(_sceneTransitionService);
            ServiceLocator.RegisterInstance(_sceneTransitionUsecase);
            ServiceLocator.RegisterInstance(_sceneTransitionController);
            _ownsRegistrations = true;

            if (_loadingScreenView == null)
            {
                Debug.LogError(
                    $"[{nameof(SceneTransitionInitializer)}] " +
                    $"{nameof(_loadingScreenView)}が設定されていません。",
                    this);
            }
            else
            {
                _loadingScreenView.Initialize(
                    _loadingScreenController);
            }

            InitializeViews(_sceneTransitionController);
            return true;
        }

        /// <summary>
        ///     登録済みサービスを解除する。
        /// </summary>
        public override void Shutdown()
        {
            if (!_ownsRegistrations)
            {
                return;
            }

            if (ServiceLocator.TryGetInstance(out SceneTransitionController registeredController)
                && ReferenceEquals(registeredController, _sceneTransitionController))
            {
                ServiceLocator.UnregisterInstance<SceneTransitionController>();
            }

            if (ServiceLocator.TryGetInstance(out SceneTransitionUsecase registeredUsecase)
                && ReferenceEquals(registeredUsecase, _sceneTransitionUsecase))
            {
                ServiceLocator.UnregisterInstance<SceneTransitionUsecase>();
            }

            if (ServiceLocator.TryGetInstance<ISceneTransitionService>(out var registeredService)
                && ReferenceEquals(registeredService, _sceneTransitionService))
            {
                ServiceLocator.UnregisterInstance<ISceneTransitionService>();
            }

            if (ServiceLocator.TryGetInstance<ILoadingOperationExecutor>(out var registeredLoadingOperationExecutor)
                && ReferenceEquals(registeredLoadingOperationExecutor, _loadingOperationExecutor))
            {
                ServiceLocator.UnregisterInstance<ILoadingOperationExecutor>();
            }

            if (ServiceLocator.TryGetInstance<ILoadingSessionFactory>(out var registeredLoadingSessionFactory)
                && ReferenceEquals(registeredLoadingSessionFactory, _loadingScreenController))
            {
                ServiceLocator.UnregisterInstance<ILoadingSessionFactory>();
            }

            if (ServiceLocator.TryGetInstance(out LoadingScreenController registeredLoadingScreenController)
                && ReferenceEquals(registeredLoadingScreenController, _loadingScreenController))
            {
                ServiceLocator.UnregisterInstance<LoadingScreenController>();
            }

            _loadingScreenController = null;
            _loadingOperationExecutor = null;
            _sceneTransitionService = null;
            _sceneTransitionUsecase = null;
            _sceneTransitionController = null;
            _ownsRegistrations = false;
        }

        /// <summary>
        ///     シーン遷移を使用するViewを初期化する。
        /// </summary>
        /// <param name="controller">
        ///     シーン遷移コントローラー。
        /// </param>
        private void InitializeViews(
            SceneTransitionController controller)
        {
            if (_debugView != null)
            {
                _debugView.Initialize(controller);
            }
        }

        private LoadingScreenController _loadingScreenController;
        private ILoadingOperationExecutor _loadingOperationExecutor;
        private ISceneTransitionService _sceneTransitionService;
        private SceneTransitionUsecase _sceneTransitionUsecase;
        private SceneTransitionController _sceneTransitionController;
        private bool _ownsRegistrations;
    }
}
