using KillChord.Runtime.Adaptor.InGame.Enemy.EnemyAIFacadeInterface;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy.AIFacade
{
    /// <summary>
    ///     敵・ボス共通のAI用ファサード：共通情報。
    /// </summary>
    public abstract class SharedFacadeBase : MonoBehaviour, IEnemySharedFacade
    {
        /// <summary>
        ///     初期化処理。
        /// </summary>
        /// <param name="target"> 攻撃対象。 </param>
        public void Initialize(Transform target)
        {
            _target = target;
        }

        /// <summary> 攻撃対象。 </summary>
        public Transform AttackTarget => _target;

        private Transform _target;
    }
}
