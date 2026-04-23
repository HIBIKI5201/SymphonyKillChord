using KillChord.Runtime.Adaptor;
using UnityEngine;

namespace KillChord.Runtime.View
{
    public class EnemySharedFacade : MonoBehaviour, IEnemySharedFacede
    {
        public void Initialize(Transform target)
        {
            _target = target;
        }

        public Transform AttackTarget => _target;

        private Transform _target;
    }
}
