using KillChord.Runtime.Adaptor.InGame.Enemy;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy
{
    public class EnemySharedFacade : MonoBehaviour, IEnemySharedFacade
    {
        public void Initialize(Transform target)
        {
            _target = target;
        }

        public Transform AttackTarget => _target;

        private Transform _target;
    }
}
