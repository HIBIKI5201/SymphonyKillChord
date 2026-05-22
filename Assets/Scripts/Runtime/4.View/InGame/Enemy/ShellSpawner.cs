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
        /// <summary>
        ///     初期化処理。
        /// </summary>
        /// <param name="shellPool"></param>
        public void Initialize(IShellPool shellPool)
        {
            _shellPool = shellPool;
        }
        /// <summary>
        ///     砲弾を生成する。
        /// </summary>
        /// <param name="enemyBattleState"></param>
        public void SpawnShell(EnemyBattleState enemyBattleState)
        {
            IShellLifeCycle shell = _shellPool.GetShell();
            shell.Activate(enemyBattleState);
        }

        [SerializeField]
        private EnemyMoveView _enemyMoveView;
        private IShellPool _shellPool;
    }
}
