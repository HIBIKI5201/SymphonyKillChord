using KillChord.Runtime.Adaptor.InGame.Mission;
using KillChord.Runtime.Adaptor.InGame.StageSelect;
using KillChord.Runtime.Adaptor.OutGame.StageSelect;
using KillChord.Runtime.Adaptor.Persistent.Load;
using KillChord.Runtime.Adaptor.Persistent.SceneManagement;
using KillChord.Runtime.Application.Persistent.Load;
using KillChord.Runtime.Application.Persistent.SceneManagement;
using KillChord.Runtime.Composition.Persistent.Bootstrap;
using KillChord.Runtime.Composition.Persistent.Input;
using KillChord.Runtime.InfraStructure.Persistent.SceneManagement;
using KillChord.Runtime.View.OutGame.Screen;
using KillChord.Runtime.View.Persistent.Load;
using KillChord.Runtime.View.Persistent.SceneManagement;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

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

        /// <summary> 回復の基盤として使用する常駐シーン名です。 </summary>
        public string PersistentSceneName => gameObject.scene.name;

        /// <summary>
        ///     専用出撃の失敗時、常駐シーンで一つだけ手動復帰画面を所有します。
        /// </summary>
        public void ShowScenarioBattleRecovery(string inGameSceneName, params string[] ownedScenes)
        {
            _sceneTransitionController.PersistentLifetimeToken.ThrowIfCancellationRequested();
            if (_recoveryView != null) { return; }
            _recoveryInGameSceneName = inGameSceneName;
            _recoveryScenes = ownedScenes;
            if (ServiceLocator.TryGetInstance(out InputComposition input))
            {
                input.GetInputMapController.DisableAll();
            }
            foreach (UIDocument document in FindObjectsByType<UIDocument>(FindObjectsSortMode.None))
            {
                if (Array.IndexOf(ownedScenes, document.gameObject.scene.name) >= 0) { document.enabled = false; }
            }
            _recoveryView = gameObject.AddComponent<OutGameInitializationFailureView>();
            _recoveryView.Initialize("タイトルへ戻る");
            _recoveryView.OnRecoveryRequested += RecoveryRequestedHandler;
        }

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
                _sceneTransitionController = existingController;
                ServiceLocator.RegisterInstance(this);

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
            _persistentLifetimeCancellation = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            _sceneTransitionController = new SceneTransitionController(
                _sceneTransitionUsecase,
                _persistentLifetimeCancellation.Token, _sceneInitializationTimeoutFrameCount);

            ServiceLocator.RegisterInstance(_loadingScreenController);
            ServiceLocator.RegisterInstance<ILoadingSessionFactory>(_loadingScreenController);
            ServiceLocator.RegisterInstance<ILoadingOperationExecutor>(_loadingOperationExecutor);
            ServiceLocator.RegisterInstance<ISceneTransitionService>(_sceneTransitionService);
            ServiceLocator.RegisterInstance<ISceneInitializationReadiness>(_sceneInitializationReadiness);
            ServiceLocator.RegisterInstance(_sceneTransitionUsecase);
            ServiceLocator.RegisterInstance(_sceneTransitionController);
            ServiceLocator.RegisterInstance(this);
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
            _persistentLifetimeCancellation?.Cancel();
            if (_ownsRegistrations) { _sceneTransitionController?.EndScenarioBattleSortie(); }
            ClearRecoveryView();
            if (ServiceLocator.TryGetInstance(out SceneTransitionInitializer registeredInitializer)
                && ReferenceEquals(registeredInitializer, this))
            {
                ServiceLocator.UnregisterInstance<SceneTransitionInitializer>();
            }
            if (!_ownsRegistrations)
            {
                return;
            }

            if (ServiceLocator.TryGetInstance(out PendingNodeTransitionState pending))
            {
                pending.Clear();
                if (ServiceLocator.TryGetInstance(out PendingNodeTransitionState current)
                    && ReferenceEquals(current, pending))
                {
                    ServiceLocator.UnregisterInstance<PendingNodeTransitionState>();
                }
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
            _persistentLifetimeCancellation?.Dispose();
            _persistentLifetimeCancellation = null;
        }

        private const int DEFAULT_SCENE_INITIALIZATION_TIMEOUT_FRAME_COUNT = 3600;
        private const string RECOVERY_TITLE_SCENE_NAME = "Title";

        [SerializeField, Tooltip("シーン遷移中に表示するロード画面")]
        private LoadingScreenView _loadingScreenView;

        [SerializeField, Tooltip("シーン遷移確認用のデバッグView")]
        private SceneTransitionView _debugView;

        [SerializeField, Min(1), Tooltip("シーン初期化完了を待機する最大フレーム数")]
        private int _sceneInitializationTimeoutFrameCount =
            DEFAULT_SCENE_INITIALIZATION_TIMEOUT_FRAME_COUNT;

        [SerializeField, Min(0f), Tooltip("ロード画面の最低表示時間")]
        private float _minimumLoadingScreenDisplayTime = 0.8f;

        private OutGameInitializationFailureView _recoveryView;
        private string _recoveryInGameSceneName;
        private string[] _recoveryScenes;
        private bool _isRecovering;
        private LoadingScreenController _loadingScreenController;
        private ILoadingOperationExecutor _loadingOperationExecutor;
        private ISceneTransitionService _sceneTransitionService;
        private ISceneInitializationReadiness _sceneInitializationReadiness;
        private SceneTransitionUsecase _sceneTransitionUsecase;
        private SceneTransitionController _sceneTransitionController;
        private bool _ownsRegistrations;
        private CancellationTokenSource _persistentLifetimeCancellation;

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

        /// <summary>
        ///     手動復帰の二重操作を拒否し、失敗時は同じ画面で再操作を受け付けます。
        /// </summary>
        private async void RecoveryRequestedHandler()
        {
            if (_isRecovering || _recoveryView == null) { return; }
            SceneTransitionController transition = _sceneTransitionController;
            if (transition.PersistentLifetimeToken.IsCancellationRequested) { return; }
            _isRecovering = true;
            _recoveryView.SetBusy(true);
            try
            {
                if (!await RecoverScenarioBattleToTitleAsync()
                    && this != null && _recoveryView != null)
                {
                    _recoveryView.ShowRecoveryFailed();
                }
            }
            catch (OperationCanceledException) when (transition.PersistentLifetimeToken.IsCancellationRequested)
            {
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                if (this != null && _recoveryView != null) { _recoveryView.ShowRecoveryFailed(); }
            }
            finally
            {
                if (this != null && !transition.PersistentLifetimeToken.IsCancellationRequested)
                {
                    _isRecovering = false;
                    if (_recoveryView != null) { _recoveryView.SetBusy(false); }
                }
            }
        }

        /// <summary>
        ///     初期化終了を確認し、残存シーンを整理して既存の Title 導線へ戻します。
        /// </summary>
        private async Task<bool> RecoverScenarioBattleToTitleAsync()
        {
            SceneTransitionController transition = _sceneTransitionController;
            if (!await transition.SettleScenarioBattleInitializationAsync(
                    true, SceneManager.GetSceneByName(_recoveryInGameSceneName).isLoaded)) { return false; }

            string anchor = PersistentSceneName;
            foreach (string sceneName in _recoveryScenes)
            {
                if (string.IsNullOrWhiteSpace(sceneName) || !SceneManager.GetSceneByName(sceneName).isLoaded) { continue; }
                if (!await transition.UnloadAndSetActiveWithPersistentLifetimeAsync(sceneName, anchor)
                    || SceneManager.GetSceneByName(sceneName).isLoaded) { return false; }
            }
            // 専用受付を保持したまま失敗選択を整理し、Title の新しい選択を消しません。
            if (ServiceLocator.TryGetInstance(out PendingNodeTransitionState pending)) { pending.Clear(); }
            if (ServiceLocator.TryGetInstance(out SelectedBattleStageState battle)) { battle.Clear(); }
            if (ServiceLocator.TryGetInstance(out SelectedMissionState mission)) { mission.Clear(); }

            bool success = SceneManager.GetSceneByName(RECOVERY_TITLE_SCENE_NAME).isLoaded
                ? await transition.ReloadSceneWithPersistentLifetimeAsync(RECOVERY_TITLE_SCENE_NAME)
                : await transition.ChangeSceneWithPersistentLifetimeAsync(null, RECOVERY_TITLE_SCENE_NAME);
            transition.PersistentLifetimeToken.ThrowIfCancellationRequested();
            bool hasTitleFailureView = false;
            if (!success && SceneManager.GetSceneByName(RECOVERY_TITLE_SCENE_NAME).isLoaded)
            {
                // 初期化結果待機後も、既存の失敗画面が接続されるまでは専用受付を保持します。
                await _sceneInitializationReadiness.WaitForReadyAsync(
                    RECOVERY_TITLE_SCENE_NAME, transition.PersistentLifetimeToken);
                for (int frame = 0; frame < _sceneInitializationTimeoutFrameCount; frame++)
                {
                    transition.PersistentLifetimeToken.ThrowIfCancellationRequested();
                    foreach (OutGameInitializationFailureView view in FindObjectsByType<OutGameInitializationFailureView>(FindObjectsSortMode.None))
                    {
                        if (view.isActiveAndEnabled && view.gameObject.scene.name == RECOVERY_TITLE_SCENE_NAME)
                        {
                            hasTitleFailureView = true;
                            break;
                        }
                    }
                    if (hasTitleFailureView) { break; }
                    await Awaitable.NextFrameAsync(transition.PersistentLifetimeToken);
                }
            }
            if (success || hasTitleFailureView)
            {
                transition.EndScenarioBattleSortie();
                ClearRecoveryView();
            }
            return success;
        }

        /// <summary>
        ///     専用失敗の終端または常駐終了で画面と購読を解除します。
        /// </summary>
        private void ClearRecoveryView()
        {
            if (_recoveryView != null)
            {
                _recoveryView.OnRecoveryRequested -= RecoveryRequestedHandler;
                _recoveryView.enabled = false;
                Destroy(_recoveryView);
                _recoveryView = null;
            }
            _recoveryScenes = null;
            _recoveryInGameSceneName = null;
        }

    }
}
