using UnityEngine;

namespace KillChord.Runtime.Application.InGame.Enemy
{
    /// <summary>
    ///     プレイヤーに攻撃できる最も近い位置を取得するインタフェース。
    /// </summary>
    public interface INearestAttackPositionSearchRepository
    {
        /// <summary>
        ///     プレイヤーに攻撃できる最も近い位置を取得する。
        /// </summary>
        /// <param name="enemyPosition"></param>
        /// <param name="playerPosition"></param>
        /// <param name="attackRangeMin"></param>
        /// <returns></returns>
        public Vector3 FindNearestAttackPosition(Vector3 enemyPosition, Vector3 playerPosition, float attackRangeMin);
    }
}
