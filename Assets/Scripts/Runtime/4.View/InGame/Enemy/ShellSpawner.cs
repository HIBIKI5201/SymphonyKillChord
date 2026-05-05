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
        public void SpawnShell(EnemyBattleState enemyBattleState)
        {
            ShellView shellView = Instantiate(_shellPrefab, _enemyMoveView.GetTargetTransform().position, Quaternion.identity);
            ServiceLocator.GetInstance<IShellInitializer>().Initialize(shellView, enemyBattleState, _enemyMoveView);
        }

        [SerializeField]
        private ShellView _shellPrefab;
        [SerializeField]
        private EnemyMoveView _enemyMoveView;
    }
}
