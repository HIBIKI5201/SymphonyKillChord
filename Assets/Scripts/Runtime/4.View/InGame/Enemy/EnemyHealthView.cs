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
        /// <param name="damageNumberPoolView"> ダメージ数値のプールView。 </param>
        public void Initialize(IHealthHudPresenter presenter, DamageNumberPoolView damageNumberPoolView)
        {
            _presenter = presenter;
            _damageNumberPoolView = damageNumberPoolView;
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
            if (_damageNumberPoolView == null)
            {
                Debug.LogError("[EnemyHealthView] DamageNumberPoolView の参照がありません。", this);
                return;
            }

            if (_damageNumberSpawnPoint == null)
            {
                Debug.LogError("[EnemyHealthView] DamageNumberSpawnPoint の参照がありません。", this);
                return;
            }

            _damageNumberPoolView.ShowDamage(
                dto,
                GetDamageNumberPos(),
                _damageNumberSpawnPoint.rotation);
        }

        private void OnDestroy()
        {
            _presenter?.Dispose();
        }

        [SerializeField, Tooltip("HP HUDのView")] private HealthHudView _healthHudView;
        [SerializeField, Tooltip("ダメージ数値の生成位置")] private Transform _damageNumberSpawnPoint;
        [SerializeField, Min(0f), Tooltip("ダメージ数値をカメラ側へ寄せる距離。")]
        private float _damageNumberCameraOffset = 0.2f;
        private IHealthHudPresenter _presenter;
        private DamageNumberPoolView _damageNumberPoolView;

        /// <summary>
        ///     ダメージ数値の表示位置を取得する。
        /// </summary>
        /// <returns> ダメージ数値の表示位置。 </returns>
        private Vector3 GetDamageNumberPos()
        {
            Vector3 pos = _damageNumberSpawnPoint.position;
            UnityEngine.Camera targetCamera = UnityEngine.Camera.main;

            if (targetCamera == null)
            {
                return pos;
            }

            Vector3 toCamera =
                targetCamera.transform.position - pos;

            if (toCamera.sqrMagnitude <= Mathf.Epsilon)
            {
                return pos;
            }

            return pos += toCamera.normalized * _damageNumberCameraOffset;
        }
    }
}
