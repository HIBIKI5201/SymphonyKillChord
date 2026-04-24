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
        public void MoveToAttack()
        {
            _moveView.MoveToAttack();
        }
        public void StopMoving()
        {
            _moveView.StopMoving();
        }

        private EnemyMoveView _moveView;
    }
}
