using UnityEngine;

namespace Mock.TPS
{
    public class PlayerAttacker
    {
        public PlayerAttacker(PlayerStatus status)
        {
            _status = status;
        }

        public void FindAttackTarget(Vector3 origin, Vector3 direction)
        {
            if (Physics.Raycast(origin, direction, out RaycastHit hitInfo, _status.AttackRange))
            {

            }
        }

        private readonly PlayerStatus _status;
    }
}
