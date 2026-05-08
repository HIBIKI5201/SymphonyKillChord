using KillChord.Runtime.Adaptor.InGame.Enemy;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     敵AI用ファサード：共通情報。
    /// </summary>
    public class EnemySharedFacade : MonoBehaviour, IEnemySharedFacade
    {
        /// <summary>
        ///     初期化処理。
        /// </summary>
        /// <param name="target"></param>
        public void Initialize(Transform target)
        {
            _target = target;
        }

        /// <summary> 敵の攻撃対象 </summary>
        public Transform AttackTarget => _target;

        private Transform _target;
    }
}
