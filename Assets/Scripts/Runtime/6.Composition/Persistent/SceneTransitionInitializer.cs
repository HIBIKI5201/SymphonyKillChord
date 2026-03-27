using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Application;
using KillChord.Runtime.Structure;
using KillChord.Runtime.View;
using UnityEngine;

namespace KillChord.Runtime.Composition
{
    /// <summary>
    ///     シーン遷移機能の初期化を行うクラス。
    /// </summary>
    public class SceneTransitionInitializer : MonoBehaviour
    {
        [SerializeField] private SceneTransitionView _debugView;

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
