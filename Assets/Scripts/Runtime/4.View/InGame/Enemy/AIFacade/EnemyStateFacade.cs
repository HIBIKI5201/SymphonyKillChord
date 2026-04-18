using KillChord.Runtime.Adaptor;
using UnityEngine;

namespace KillChord.Runtime.View
{
    public class EnemyStateFacade : MonoBehaviour, IEnemyStateFacade
    {
        public void Initialize(EnemyAIController aiController, Transform target, EnemyRaycastDetectView raycastDetectView)
        {
            _aiController = aiController;
            _target = target;
            _raycastDetectView = raycastDetectView;
        }
        public bool IsTargetInAttackRange => _aiController.IsPlayerInAttackRange(transform.position, _target.transform.position);

        public bool IsSightClearToAim => _raycastDetectView.CanRaycastHitTarget;

        public bool IsAttacking => _aiController.IsAttacking;

        private EnemyAIController _aiController;
        private Transform _target;
        private EnemyRaycastDetectView _raycastDetectView;
    }
}
