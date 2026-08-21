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

        private const int DEFAULT_SCENE_INITIALIZATION_TIMEOUT_FRAME_COUNT = 3600;

        [SerializeField, Tooltip("シーン遷移中に表示するロード画面")]
        private LoadingScreenView _loadingScreenView;

        [SerializeField, Tooltip("シーン遷移確認用のデバッグView")]
        private SceneTransitionView _debugView;

        [SerializeField, Min(1), Tooltip("シーン初期化完了を待機する最大フレーム数")]
        private int _sceneInitializationTimeoutFrameCount =
            DEFAULT_SCENE_INITIALIZATION_TIMEOUT_FRAME_COUNT;

        [SerializeField, Min(0f), Tooltip("ロード画面の最低表示時間")]
        private float _minimumLoadingScreenDisplayTime = 0.8f;

        /// <summary>
        ///     シーン遷移システムを構築して登録する。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Build()
        {
            // 既に SceneTransitionController が存在する場合はそれを使用し、存在しない場合は新たに作成して登録する。
            bool hasExistingController =
                ServiceLocator.TryGetInstance<SceneTransitionController>(out var existingController);
            bool hasExistingLoadingController =
                ServiceLocator.TryGetInstance(out LoadingScreenController existingLoadingScreenController);

            if (hasExistingController || hasExistingLoadingController)
            {
                if (!hasExistingController
                    || !hasExistingLoadingController
                    || !ServiceLocator.TryGetInstance<ISceneInitializationReadiness>(out var existingReadiness))
                {
                    Debug.LogError(
                        $"[{nameof(SceneTransitionInitializer)}] " +
                        "既存のシーン遷移サービス登録が不完全です。",
                        this);
                    return false;
                }

                _sceneInitializationReadiness = existingReadiness;

                if (_loadingScreenView != null)
                {
                    _loadingScreenView.Initialize(
                        existingLoadingScreenController);
                }

                InitializeViews(existingController);
                return true;
            }

            _loadingScreenController = new LoadingScreenController();

            _loadingOperationExecutor = new LoadingOperationExecutor(
                _loadingScreenController, _minimumLoadingScreenDisplayTime);
            _sceneTransitionService = new SceneTransitionService();
            _sceneInitializationReadiness = new SceneInitializationReadinessRegistry(
                _sceneInitializationTimeoutFrameCount);
            _sceneTransitionUsecase = new SceneTransitionUsecase(
                _sceneTransitionService,
                _loadingOperationExecutor,
                _sceneInitializationReadiness);
            _sceneTransitionController = new SceneTransitionController(_sceneTransitionUsecase);

            ServiceLocator.RegisterInstance(_loadingScreenController);
            ServiceLocator.RegisterInstance<ILoadingSessionFactory>(_loadingScreenController);
            ServiceLocator.RegisterInstance<ILoadingOperationExecutor>(_loadingOperationExecutor);
            ServiceLocator.RegisterInstance<ISceneTransitionService>(_sceneTransitionService);
            ServiceLocator.RegisterInstance<ISceneInitializationReadiness>(_sceneInitializationReadiness);
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

            if (ServiceLocator.TryGetInstance<ISceneInitializationReadiness>(out var registeredReadiness)
                && ReferenceEquals(registeredReadiness, _sceneInitializationReadiness))
            {
                ServiceLocator.UnregisterInstance<ISceneInitializationReadiness>();
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
            _sceneInitializationReadiness = null;
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
        private ISceneInitializationReadiness _sceneInitializationReadiness;
        private SceneTransitionUsecase _sceneTransitionUsecase;
        private SceneTransitionController _sceneTransitionController;
        private bool _ownsRegistrations;
    }
}
