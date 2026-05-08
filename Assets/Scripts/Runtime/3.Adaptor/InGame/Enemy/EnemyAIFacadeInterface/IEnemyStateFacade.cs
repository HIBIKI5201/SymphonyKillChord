using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Enemy
{
    /// <summary>
    ///     敵AI用ファサード：状態判定系。
    /// </summary>
    public interface IEnemyStateFacade
    {
        /// <summary>
        ///     目標が自分の攻撃範囲内か。
        /// </summary>
        /// <returns></returns>
        public bool IsTargetInAttackRange { get; }
        /// <summary>
        ///     目標と自分の間に障害物がないか。
        /// </summary>
        /// <returns></returns>
        public bool IsSightClearToAim { get; }
        /// <summary>
        ///     攻撃中であるか。
        /// </summary>
        public bool IsAttacking { get; }
        /// <summary>
        ///     硬直中か。
        /// </summary>
        public bool IsStunned { get; }
        /// <summary>
        ///     硬直発生
        /// </summary>
        public void Stunned();
        /// <summary>
        ///     硬直回復。
        /// </summary>
        public void StunRecover();
    }
}
