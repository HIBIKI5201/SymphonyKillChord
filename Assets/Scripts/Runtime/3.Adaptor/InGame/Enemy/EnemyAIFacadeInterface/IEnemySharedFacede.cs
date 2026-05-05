using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Enemy
{
    /// <summary>
    ///     敵AI用ファサード：共通情報。
    /// </summary>
    public interface IEnemySharedFacede
    {
        /// <summary>
        ///     敵の攻撃対象。
        /// </summary>
        public Transform AttackTarget {get;}
    }
}
