using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Application;
using KillChord.Runtime.Domain;
using KillChord.Runtime.InfraStructure;
using KillChord.Runtime.View.InGame;
using UnityEngine;

namespace KillChord.Runtime.View
{
    /// <summary>
    ///     敵移動の依存関係を構築する。
    /// </summary>
    public class EnemyMoveDebugInitializer : MonoBehaviour
    {
        [SerializeField] private EnemyMoveData _moveData;

        [SerializeField] private EnemyMoveView _view;
        [SerializeField] private NavMeshEnemyAgent _navigationAgent;

        private void Awake()
        {
            // Factory
            EnemyFactory factory = new EnemyFactory();

            // Domain生成
            EnemyMoveSpec spec = factory.CreateEnemyMoveSpec(_moveData);

            // UseCase
            EnemyMoveUsecase useCase = new EnemyMoveUsecase(
                spec,
                _navigationAgent);

            // Controller
            EnemyMoveController controller = new EnemyMoveController(useCase);

            // View接続
            _view.Initialize(controller);
        }
    }
}
