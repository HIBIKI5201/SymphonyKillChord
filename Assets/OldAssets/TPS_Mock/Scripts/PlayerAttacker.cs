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
                Transform target = hitInfo.transform;

                // 無視タグのチェック。
                if (target.CompareTag(_config.IgnoreAttackTagName)) { return null; }

                return target.GetComponent<ICharacter>();
            }

            return null;
        }

        public void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(_camera.position, _camera.forward * _status.AttackRange);
        }

        private readonly PlayerStatus _status;
        private readonly PlayerConfig _config;
        private readonly Transform _camera;
    }
}
