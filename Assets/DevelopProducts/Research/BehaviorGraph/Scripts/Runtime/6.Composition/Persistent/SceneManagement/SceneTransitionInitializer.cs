using DevelopProducts.BehaviorGraph.Runtime.Adaptor.Persistent.SceneManagement;
using DevelopProducts.BehaviorGraph.Runtime.Application.Persistent.SceneManagement;
using DevelopProducts.BehaviorGraph.Runtime.InfraStructure.Persistent.SceneManagement;
using DevelopProducts.BehaviorGraph.Runtime.View;
using UnityEngine;

namespace DevelopProducts.BehaviorGraph.Runtime.Composition.Persistent.SceneManagement
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
