using KillChord.Runtime.Adaptor;
using KillChord.Runtime.View.InGame;
using UnityEngine;

namespace KillChord.Runtime.View
{
    public class EnemyMovementAIFacade : MonoBehaviour, IEnemyMovementAIFacade
    {
        public void Initialize(EnemyMoveView moveView)
        {
            _moveView = moveView;
        }
        public void ChaseTarget()
        {
            _moveView.ChaseTarget();
        }
        public void StopChasing()
        {
            _moveView.StopChasing();
        }

        private EnemyMoveView _moveView;
    }
}
