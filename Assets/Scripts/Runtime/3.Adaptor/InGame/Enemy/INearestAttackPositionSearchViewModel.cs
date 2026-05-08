using UnityEngine;
using UnityEngine.AI;

namespace KillChord.Runtime.Adaptor.InGame.Enemy
{
    /// <summary>
    ///     最も近い攻撃可能な場所を検索するViewModelインターフェース。
    /// </summary>
    public interface INearestAttackPositionSearchViewModel
    {
        /// <summary>
        ///     最も近い攻撃可能な場所を検索する。
        /// </summary>
        /// <param name="enemyPosition"></param>
        /// <param name="playerPosition"></param>
        /// <param name="attackRangeMin"></param>
        /// <returns></returns>
        public Vector3 FindNearestAttackPosition(Vector3 enemyPosition, Vector3 playerPosition, float attackRangeMin);
    }
}
