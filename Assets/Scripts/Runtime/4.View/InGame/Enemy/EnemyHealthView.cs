using KillChord.Runtime.Adaptor.InGame.Enemy;
using KillChord.Runtime.Adaptor.InGame.UI;
using KillChord.Runtime.View.InGame.UI;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     敵のHP表示関連View。
    /// </summary>
    public class EnemyHealthView : MonoBehaviour, IDamageNumber
    {
        /// <summary>
        ///     初期化する。
        /// </summary>
        /// <param name="presenter"> HP HUDのPresenter。 </param>
        public void Initialize(IHealthHudPresenter presenter)
        {
            _presenter = presenter;
        }

        /// <summary>
        ///     ViewModelをバインドする。
        /// </summary>
        /// <param name="viewModel"> HP HUDのViewModel。 </param>
        public void Bind(IHealthHudViewModel viewModel)
        {
            if (_healthHudView == null)
            {
                Debug.LogError("[EnemyHealthView] HealthHudViewの参照がありません。", this);
                return;
            }

            _healthHudView.Bind(viewModel);
        }

        /// <summary>
        ///     ダメージ数値を表示する。
        /// </summary>
        /// <param name="dto"> ダメージ数値のDTO。 </param>
        public void ShowDamage(in DamageNumberDTO dto)
        {
            if (_damageNumberPrefab == null)
            {
                Debug.LogError("[EnemyHealthView] DamageNumberView のPrefab参照がありません。", this);
                return;
            }

            DamageNumberView view = Instantiate(_damageNumberPrefab, _damageNumberSpawnPoint);
            MoveTowardCamera(view.transform);
            view.Play(dto);
        }

        private void OnDestroy()
        {
            _presenter?.Dispose();
        }

        [SerializeField, Tooltip("HP HUDのView")] private HealthHudView _healthHudView;
        [SerializeField, Tooltip("ダメージ数値の生成位置")] private Transform _damageNumberSpawnPoint;
        [SerializeField, Tooltip("ダメージ数値のPrefab")] private DamageNumberView _damageNumberPrefab;
        [SerializeField, Min(0f), Tooltip("ダメージ数値をカメラ側へ寄せる距離。")]
        private float _damageNumberCameraOffset = 0.2f;
        private IHealthHudPresenter _presenter;

        /// <summary>
        ///     ダメージ数値をカメラ方向へ移動する。
        /// </summary>
        /// <param name="target">移動するダメージ数値のTransform。</param>
        private void MoveTowardCamera(Transform target)
        {
            UnityEngine.Camera targetCamera = UnityEngine.Camera.main;

            if (targetCamera == null)
            {
                return;
            }

            Vector3 toCamera =
                targetCamera.transform.position - target.position;

            if (toCamera.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            target.position +=
                toCamera.normalized * _damageNumberCameraOffset;
        }
    }
}
