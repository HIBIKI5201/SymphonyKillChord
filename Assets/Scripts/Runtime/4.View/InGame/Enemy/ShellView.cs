using KillChord.Runtime.Adaptor.InGame.Enemy;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     砲弾のView。
    /// </summary>
    public class ShellView : MonoBehaviour, IShellViewModel
    {
        /// <summary>
        ///     初期化処理。
        /// </summary>
        /// <param name="detonatePosition"></param>
        /// <param name="controller"></param>
        /// <param name="shellSpecPresenter"></param>
        public void Initialize(Vector3 detonatePosition, ShellController controller, ShellSpecPresenter shellSpecPresenter)
        {
            _detonatePosition = detonatePosition;
            _controller = controller;
            _shellSpecPresenter = shellSpecPresenter;

            // TODO 一時的な攻撃予兆表示。今後素材を差し替える
            _overlapResults = new Collider[1];
            _indicator.material.color = new Color(1, 0, 0, 0.1f);
            _indicator.transform.localScale = new Vector3(_shellSpecPresenter.ExplosionRadius * 2, _indicator.transform.localScale.y, _shellSpecPresenter.ExplosionRadius * 2);
        }

        /// <summary>
        ///     砲弾爆発の処理。
        /// </summary>
        public void Detonate()
        {
            // TODO 爆発エフェクトなど
            _controller.Dispose();
            Destroy(gameObject);
        }

        /// <summary>
        ///     爆発範囲内に攻撃目標がいるか判定する。
        /// </summary>
        /// <returns></returns>
        public bool FindDamageTarget()
        {
            int hits = Physics.OverlapSphereNonAlloc(_detonatePosition, _shellSpecPresenter.ExplosionRadius, _overlapResults, _damageLayer);
            return hits > 0;
        }

        [SerializeField]
        private LayerMask _damageLayer;
        [SerializeField]
        private Renderer _indicator;

        private ShellController _controller;
        private Vector3 _detonatePosition;
        private Collider[] _overlapResults;
        private ShellSpecPresenter _shellSpecPresenter;
    }
}
