using UnityEngine;

namespace Mock.TPS
{
    public class PlayerAttacker
    {
        public PlayerAttacker(PlayerStatus status, PlayerConfig config, Transform camera)
        {
            _status = status;
            _config = config;
            _camera = camera;
        }

        public void Attack()
        {
            ICharacter target = FindAttackTarget(_camera.position, _camera.forward);
            if (target == null) { return; }
            Debug.Log($"Attack Target: {target.gameObject.name}");
        }

        public ICharacter FindAttackTarget(Vector3 origin, Vector3 direction)
        {
            if (Physics.Raycast(origin, direction, out RaycastHit hitInfo, _status.AttackRange))
            {
                return hitInfo.transform.GetComponent<ICharacter>();
            }

            return null;
        }

        private readonly PlayerStatus _status;
        private readonly PlayerConfig _config;
        private readonly Transform _camera;
    }
}
