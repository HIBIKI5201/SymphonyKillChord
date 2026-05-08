using KillChord.Runtime.Application.InGame.Enemy;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Enemy
{
    /// <summary>
    ///     プレイヤーに攻撃できる最も近い位置を取得する。
    /// </summary>
    public class NearestAttackPositionSearchController : INearestAttackPositionSearchRepository
    {
        public NearestAttackPositionSearchController(INearestAttackPositionSearchViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        /// <summary>
        ///     プレイヤーに攻撃できる最も近い位置を取得する。
        /// </summary>
        /// <param name="enemyPosition"></param>
        /// <param name="playerPosition"></param>
        /// <param name="attackRangeMin"></param>
        /// <returns></returns>
        public Vector3 FindNearestAttackPosition(Vector3 enemyPosition, Vector3 playerPosition, float attackRangeMin)
        {
            return _viewModel.FindNearestAttackPosition(enemyPosition, playerPosition, attackRangeMin);
        }

        private INearestAttackPositionSearchViewModel _viewModel;
    }
}
