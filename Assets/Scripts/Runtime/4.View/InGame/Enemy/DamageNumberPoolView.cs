using KillChord.Runtime.Adaptor.InGame.Enemy;
using System;
using UnityEngine;
using UnityEngine.Pool;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     ダメージ表示のプールを管理するView。
    /// </summary>
    public class DamageNumberPoolView : MonoBehaviour
    {
        /// <summary>
        ///     ダメージ表示をプールから取得して表示する処理
        /// </summary>
        /// <param name="damage"> ダメージ情報 </param>
        /// <param name="position"> 表示位置 </param>
        /// <param name="rotation"> 表示回転 </param>
        public void ShowDamage(in DamageNumberDTO damage, Vector3 position, Quaternion rotation)
        {
            if (_damageNumberPrefab == null)
            {
                Debug.LogWarning("DamageNumberPrefabが設定されていません。");
                return;
            }

            EnsureInitialized();

            DamageNumberView damageNumberView = _damageNumberPool.Get();
            damageNumberView.transform.SetPositionAndRotation(position, rotation);
            damageNumberView.Play(damage, _releaseHandler);
        }

        [SerializeField, Tooltip("ダメージ表示のプレハブ")]
        private DamageNumberView _damageNumberPrefab;

        [SerializeField, Tooltip("ダメージ表示のプールの親オブジェクト")]
        private Transform _poolParent;

        [SerializeField, Tooltip("ダメージ表示の初期プールサイズ")]
        private int _initialPoolSize;

        [SerializeField, Tooltip("ダメージ表示の最大プールサイズ")]
        private int _maxPoolSize;

        private IObjectPool<DamageNumberView> _damageNumberPool;
        private Action<DamageNumberView> _releaseHandler;

        private void Awake()
        {
            EnsureInitialized();
            Prewarm();
        }

        private void OnDestroy()
        {
            _damageNumberPool?.Clear();
        }

        /// <summary>
        ///    プールの初期化を行う処理
        /// </summary>
        private void EnsureInitialized()
        {
            if (_damageNumberPool != null)
            {
                return;
            }

            _releaseHandler = Release;

            int initialPoolSize = Mathf.Max(1, _initialPoolSize);
            int maxPoolSize = Mathf.Max(initialPoolSize, _maxPoolSize);

            _damageNumberPool = new ObjectPool<DamageNumberView>(
                Create,
                HandleGetFromPool,
                HandleReleaseToPool,
                HandleDestroyPool,
                true,
                initialPoolSize,
                maxPoolSize
            );

        }

        /// <summary>
        ///     プールの事前生成を行う処理
        /// </summary>
        private void Prewarm()
        {
            if (_damageNumberPrefab == null)
            {
                return;
            }

            // プールの初期化と事前生成
            int count = Mathf.Min(Mathf.Max(1, _initialPoolSize), Mathf.Max(1, _maxPoolSize));
            DamageNumberView[] views = new DamageNumberView[count];

            for (int i = 0; i < count; i++)
            {
                views[i] = _damageNumberPool.Get();
            }

            for (int i = 0; i < count; i++)
            {
                _damageNumberPool.Release(views[i]);
            }
        }

        /// <summary>
        ///     プールから新しいダメージ表示オブジェクトを生成する処理
        /// </summary>
        /// <returns> 生成されたダメージ表示オブジェクト </returns>
        private DamageNumberView Create()
        {
            Transform parent = _poolParent != null ? _poolParent : transform;

            DamageNumberView damageNumberView = Instantiate(_damageNumberPrefab, parent);
            damageNumberView.gameObject.SetActive(false);
            return damageNumberView;
        }

        /// <summary>
        ///     プールから取得したオブジェクトを有効化する処理
        /// </summary>
        /// <param name="damageNumberView"> 有効化するダメージ表示オブジェクト </param>
        private void HandleGetFromPool(DamageNumberView damageNumberView)
        {
            if (damageNumberView != null)
            {
                damageNumberView.gameObject.SetActive(true);
            }
        }

        /// <summary>
        ///     プールに返却されたオブジェクトを無効化する処理
        /// </summary>
        /// <param name="damageNumberView"> 無効化するダメージ表示オブジェクト </param>
        private void HandleReleaseToPool(DamageNumberView damageNumberView)
        {
            if (damageNumberView != null)
            {
                damageNumberView.ResetView();
                damageNumberView.gameObject.SetActive(false);
            }
        }

        /// <summary>
        ///    プールから破棄されたオブジェクトを削除する処理
        /// </summary>
        /// <param name="damageNumberView"> 削除するダメージ表示オブジェクト </param>
        private void HandleDestroyPool(DamageNumberView damageNumberView)
        {
            if (damageNumberView != null)
            {
                Destroy(damageNumberView.gameObject);
            }
        }

        /// <summary>
        ///     ダメージ表示オブジェクトをプールに返却する処理
        /// </summary>
        /// <param name="damageNumberView"> 返却するダメージ表示オブジェクト </param>
        private void Release(DamageNumberView damageNumberView)
        {
            if (damageNumberView != null && _damageNumberPool != null)
            {
                _damageNumberPool.Release(damageNumberView);
            }
        }
    }
}
