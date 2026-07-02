using KillChord.Runtime.Adaptor.InGame.Enemy.EnemyAIFacadeInterface;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     ボスAI用ファサード：共通情報。EnemySharedFacade のボス専用複製。
    /// </summary>
    public class BossSharedFacade : MonoBehaviour, IEnemySharedFacade
    {
        /// <summary>
        ///     初期化処理。
        /// </summary>
        public void Initialize(Transform target)
        {
            _target = target;
        }

        /// <summary> ボスの攻撃対象 </summary>
        public Transform AttackTarget => _target;

        private Transform _target;
    }
}
