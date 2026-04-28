using KillChord.Runtime.Application.InGame.Enemy;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Enemy
{
    public class NearestAttackPositionSearchController : INearestAttackPositionSearchRepository
    {
        public NearestAttackPositionSearchController(INearestAttackPositionSearchViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public Vector3 FindNearestAttackPosition(Vector3 enemyPosition, Vector3 playerPosition, float attackRangeMin)
        {
            return _viewModel.FindNearestAttackPosition(enemyPosition, playerPosition, attackRangeMin);
        }

        private INearestAttackPositionSearchViewModel _viewModel;
    }
}
