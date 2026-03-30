using KillChord.Runtime.Adaptor.Persistent.SceneManagement;
using KillChord.Runtime.Application.Persistent.SceneManagement;
using KillChord.Runtime.InfraStructure.Persistent.SceneManagement;
using KillChord.Runtime.View;
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

            ISceneTransitionService service = new SceneTransitionService();
            SceneTransitionController controller = new SceneTransitionController(service);

            _debugView.Initialize(controller);
        }
    }
}
