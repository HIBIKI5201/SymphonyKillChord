using KillChord.Runtime.Adaptor.InGame.Enemy;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy
{
    public class ShellView : MonoBehaviour, IShellViewModel
    {
        public void Initialize(Vector3 detonatePosition, ShellController controller, EnemyAIController enemyAIController)
        {
            _detonatePosition = detonatePosition;
            _controller = controller;
            _enemyAIController = enemyAIController;

            _overlapResults = new Collider[1];
            _indicator.material.color = new Color(1, 0, 0, 0.1f);
            _indicator.transform.localScale = new Vector3(_damageRadius * 2, _indicator.transform.localScale.y, _damageRadius * 2);
        }

        public void Detonate()
        {
            // TODO 爆発エフェクトなど
            _controller.Dispose();
            Destroy(gameObject);
        }

        public bool FindDamageTarget()
        {
            int hits = Physics.OverlapSphereNonAlloc(_detonatePosition, _damageRadius, _overlapResults, _damageLayer);
            return hits > 0;
        }

        [SerializeField]
        private float _damageRadius = 2f;
        [SerializeField]
        private LayerMask _damageLayer;
        [SerializeField]
        private Renderer _indicator;

        private ShellController _controller;
        private EnemyAIController _enemyAIController;
        private Vector3 _detonatePosition;
        private Collider[] _overlapResults;
    }
}
