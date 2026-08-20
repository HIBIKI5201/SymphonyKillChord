using KillChord.Runtime.Adaptor.InGame.Enemy;
using KillChord.Runtime.View.InGame.Character;
using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     砲弾のView。
    /// </summary>
    public class ShellView : MonoBehaviour, IShellView
    {
        /// <summary>
        ///     初期化処理。
        /// </summary>
        /// <param name="targetTransform"> 砲弾の着弾位置に追従させる攻撃目標のTransform。 </param>
        /// <param name="shellSpecPresenter"> 爆発半径などの砲弾仕様を参照するPresenter。 </param>
        /// <param name="dedonateCallback"> 爆発時に呼び出すコールバック。 </param>
        /// <param name="systemView"> 爆発エフェクトを再生するパーティクルView。 </param>
        /// <param name="justOffsetProvider"> 爆発までの接近進捗（0〜1）を返す関数。デカールの進捗表示に使用する。 </param>
        /// <exception cref="ArgumentNullException"> 引数のいずれかがNULLの場合。 </exception>
        /// <exception cref="InvalidOperationException"> DecalProjectorまたはそのマテリアルが未アサインの場合。 </exception>
        public void Initialize(
            Transform targetTransform,
            ShellSpecPresenter shellSpecPresenter,
            Action dedonateCallback,
            ReusableParticleSystemView systemView,
            Func<float> justOffsetProvider)
        {
            if (systemView == null)
            {
                throw new ArgumentNullException(nameof(systemView), "ReusableParticleSystemViewがNULLです。");
            }

            if (_indicator == null)
            {
                throw new InvalidOperationException($"[{nameof(ShellView)}] 爆発範囲表示用のDecalProjectorがアサインされていません。");
            }

            if (_indicator.material == null)
            {
                throw new InvalidOperationException($"[{nameof(ShellView)}] 爆発範囲表示用のDecalProjectorにマテリアルがアサインされていません。");
            }

            _targetTransform = targetTransform;
            _shellSpecPresenter = shellSpecPresenter ?? throw new ArgumentNullException(nameof(shellSpecPresenter), "ShellSpecPresenterがNULLです。");
            _dedonateCallback = dedonateCallback ?? throw new ArgumentNullException(nameof(dedonateCallback), "DedonateCallbackがNULLです。");
            _systemView = systemView;
            _justOffsetProvider = justOffsetProvider ?? throw new ArgumentNullException(nameof(justOffsetProvider), "JustOffsetProviderがNULLです。");
            _overlapResults = new Collider[1];
            _material = new Material(_indicator.material);
            _indicator.material = _material;


            _indicator.size = new Vector3(_shellSpecPresenter.ExplosionRadius * 2, _shellSpecPresenter.ExplosionRadius * 2, 2);
        }

        /// <summary>
        ///     有効化処理。
        /// </summary>
        public void Activate()
        {
            if (_targetTransform == null)
            {
                Debug.LogError($"[{nameof(ShellView)}] 攻撃対象を失っています。");
                return;
            }
            // プール再利用時に前回の進捗が残らないようリセットする。
            ResetIndicatorRatio();
            transform.position = _targetTransform.position;
            _indicator.gameObject.SetActive(true);
        }

        /// <summary>
        ///     無効化処理。
        /// </summary>
        public void Deactivate()
        {
            ResetIndicatorRatio();
            _indicator.gameObject.SetActive(false);
        }

        /// <summary>
        ///     砲弾爆発の処理。
        /// </summary>
        public void Detonate()
        {
            // TODO 爆発エフェクトなど
            _systemView?.PlayAt(transform.position);
            _dedonateCallback?.Invoke();
        }

        /// <summary>
        ///     爆発範囲内に攻撃目標がいるか判定する。
        /// </summary>
        /// <returns> 爆発範囲内に攻撃目標が存在する場合はtrue。 </returns>
        public bool FindDamageTarget()
        {
            int hits = Physics.OverlapSphereNonAlloc(transform.position, _shellSpecPresenter.ExplosionRadius, _overlapResults, _damageLayer);
            return hits > 0;
        }
        /// <summary>
        ///     爆発までの接近進捗をデカールのシェーダープロパティへ適用する。
        /// </summary>
        private void LateUpdate()
        {
            if (_material == null || _justOffsetProvider == null)
            {
                return;
            }

            _material.SetFloat(DECAL_CIRCLE, _justOffsetProvider.Invoke());
        }

        /// <summary>
        ///     破棄時に複製したマテリアルを解放する。
        /// </summary>
        private void OnDestroy()
        {
            if (_material != null)
            {
                Destroy(_material);
                _material = null;
            }
        }

        [SerializeField, Tooltip("ダメージ判定のレイヤー")]
        private LayerMask _damageLayer;
        [SerializeField, Tooltip("爆発範囲表示用")]
        private DecalProjector _indicator;

        /// <summary>
        ///     デカールの接近進捗を初期値へ戻す。
        /// </summary>
        private void ResetIndicatorRatio()
        {
            if (_material == null)
            {
                return;
            }

            _material.SetFloat(DECAL_CIRCLE, 0f);
        }


        private Material _material;
        private Transform _targetTransform;
        private Collider[] _overlapResults;
        private ShellSpecPresenter _shellSpecPresenter;
        private ReusableParticleSystemView _systemView;
        private Action _dedonateCallback;
        private Func<float> _justOffsetProvider;

        private static readonly int DECAL_CIRCLE = Shader.PropertyToID("_Circle");
    }
}
