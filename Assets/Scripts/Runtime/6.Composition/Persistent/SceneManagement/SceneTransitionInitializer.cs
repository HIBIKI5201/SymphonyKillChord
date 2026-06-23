using KillChord.Runtime.Adaptor.Persistent.Load;
using KillChord.Runtime.Adaptor.Persistent.SceneManagement;
using KillChord.Runtime.Application.Persistent.Load;
using KillChord.Runtime.Application.Persistent.SceneManagement;
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
    public class SceneTransitionInitializer : MonoBehaviour
    {
        [SerializeField, Tooltip("シーン遷移中に表示するロード画面")]
        private LoadingScreenView _loadingScreenView;

        [SerializeField, Tooltip("シーン遷移確認用のデバッグView")]
        private SceneTransitionView _debugView;

        private void Awake()
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
                return;
            }

            LoadingScreenController loadingScreenController = new();

            ILoadingOperationExecutor loadingOperationExecutor = 
                new LoadingOperationExecutor(loadingScreenController);
            ISceneTransitionService service = new SceneTransitionService();
            var usecase = new SceneTransitionUsecase(service, loadingOperationExecutor);
            SceneTransitionController controller = new SceneTransitionController(usecase);

            ServiceLocator.RegisterInstance(loadingScreenController);
            ServiceLocator.RegisterInstance<ILoadingSessionFactory>(loadingScreenController);
            ServiceLocator.RegisterInstance<ILoadingOperationExecutor>(loadingOperationExecutor);
            ServiceLocator.RegisterInstance<ISceneTransitionService>(service);
            ServiceLocator.RegisterInstance(usecase);
            ServiceLocator.RegisterInstance(controller);

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
                    loadingScreenController);
            }

            InitializeViews(controller);
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
    }
}
