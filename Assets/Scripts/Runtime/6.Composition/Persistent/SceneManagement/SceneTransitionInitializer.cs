using KillChord.Runtime.Adaptor.Persistent.SceneManagement;
using KillChord.Runtime.Application.Persistent.SceneManagement;
using KillChord.Runtime.InfraStructure.Persistent.SceneManagement;
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
        [SerializeField] private SceneTransitionView _debugView; // デバッグ用のView。

        private void Awake()
        {
            if (_debugView == null)
            {
                Debug.LogError($"[{nameof(SceneTransitionInitializer)}] _debugView is not assigned.", this);
                return;
            }

            // 既に SceneTransitionController が存在する場合はそれを使用し、存在しない場合は新たに作成して登録する。
            if (ServiceLocator.TryGetInstance<SceneTransitionController>(out var existingController))
            {
                _debugView.Initialize(existingController);
                return;
            }

            ISceneTransitionService service = new SceneTransitionService();
            SceneTransitionController controller = new SceneTransitionController(service);

            ServiceLocator.RegisterInstance(controller);

            _debugView.Initialize(controller);
        }
    }
}
