using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Adaptor.InGame.Enemy;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     砲弾を生成するクラス。
    /// </summary>
    public class ShellSpawner : MonoBehaviour, IShellSpawner
    {
        public void SpawnShell(EnemyAIController enemyAIController)
        {
            ShellView shellView = Instantiate(_shellPrefab, _enemyMoveView.GetTargetTransform().position, Quaternion.identity);
            ShellController shellController = ServiceLocator.GetInstance<IShellInitializer>().InitAndGetShellController(shellView, enemyAIController);
            
            shellView.Initialize(_enemyMoveView.GetTargetTransform().position, shellController, enemyAIController);
        }

        [SerializeField]
        private ShellView _shellPrefab;
        [SerializeField]
        private EnemyMoveView _enemyMoveView;
    }
}
