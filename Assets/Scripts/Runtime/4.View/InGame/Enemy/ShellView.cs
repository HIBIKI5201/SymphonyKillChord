using KillChord.Runtime.Adaptor.InGame.Enemy;
using System;
using UnityEngine;

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
        /// <param name="detonatePosition"></param>
        /// <param name="controller"></param>
        /// <param name="shellSpecPresenter"></param>
        public void Initialize(Transform targetTransform, ShellSpecPresenter shellSpecPresenter, Action dedonateCallback)
        {
            if (shellSpecPresenter == null)
                throw new ArgumentNullException(nameof(shellSpecPresenter), "ShellSpecPresenterがNULLです。");

            _targetTransform = targetTransform;
            _shellSpecPresenter = shellSpecPresenter;
            _dedonateCallback = dedonateCallback;
            _overlapResults = new Collider[1];

            // TODO 一時的な攻撃予兆表示。今後素材を差し替える
            ChangeShellColor(ShellColor.Green);
            _indicator.transform.localScale = new Vector3(_shellSpecPresenter.ExplosionRadius * 2, _indicator.transform.localScale.y, _shellSpecPresenter.ExplosionRadius * 2);
        }

        /// <summary>
        ///     有効化処理。
        /// </summary>
        public void Activate()
        {
            if(_targetTransform == null)
            {
                Debug.LogError("[ShellView] 攻撃対象を失っています。");
                return;
            }
             ChangeShellColor(ShellColor.Green);
            transform.position = _targetTransform.position;
            _indicator.gameObject.SetActive(true);
        }

        /// <summary>
        ///     無効化処理。
        /// </summary>
        public void Deactivate()
        {
            _indicator.gameObject.SetActive(false);
        }

        /// <summary>
        ///     砲弾爆発の処理。
        /// </summary>
        public void Detonate()
        {
            // TODO 爆発エフェクトなど
            _dedonateCallback?.Invoke();
        }

        public void ChangeShellColor(ShellColor color)
        {
            Color unityColor = color switch
            {
                ShellColor.Red => Color.red,
                ShellColor.Blue => Color.blue,
                ShellColor.Green => Color.green,
                ShellColor.Yellow => Color.yellow,
                _ => Color.white
            };
            _indicator.material.color = new Color(unityColor.r, unityColor.g, unityColor.b, 1f);
            Debug.Log($"<color={color.ToString()}>[ShellView] 砲弾の色を{color}に変更</color>");
        }

        /// <summary>
        ///     爆発範囲内に攻撃目標がいるか判定する。
        /// </summary>
        /// <returns></returns>
        public bool FindDamageTarget()
        {
            int hits = Physics.OverlapSphereNonAlloc(transform.position, _shellSpecPresenter.ExplosionRadius, _overlapResults, _damageLayer);
            return hits > 0;
        }

        [SerializeField, Tooltip("ダメージ判定のレイヤー")]
        private LayerMask _damageLayer;
        [SerializeField, Tooltip("爆発範囲表示用")]
        private Renderer _indicator;

        private Transform _targetTransform;
        private Collider[] _overlapResults;
        private ShellSpecPresenter _shellSpecPresenter;
        private Action _dedonateCallback;
    }
}
