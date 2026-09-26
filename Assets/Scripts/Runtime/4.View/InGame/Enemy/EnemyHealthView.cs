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
        /// <param name="damageNumberPoolView"> ダメージ数値のプール。 </param>
        public void Initialize(
            IHealthHudPresenter presenter,
            DamageNumberPoolView damageNumberPoolView)
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

            Vector3 position = GetDamageNumberPos();
            Quaternion rotation = GetDamageNumberRotation(position);

            _damageNumberPoolView.ShowDamage(dto, position, rotation);
        }

        private void OnDestroy()
        {
            _presenter?.Dispose();
        }

        [SerializeField, Tooltip("HP HUDのView")]
        private HealthHudView _healthHudView;

        [SerializeField, Tooltip("ダメージ数値の基準となる敵の頭位置")]
        private Transform _damageNumberSpawnPoint;

        [SerializeField, Min(0f), Tooltip("敵の頭から円の中心までのオフセット距離")]
        private float _damageNumberCircleCenterOffset = 2f;

        [SerializeField, Min(0f), Tooltip("ダメージ数値を配置する半円の半径")]
        private float _damageNumberCircleRadius = 3f;

        [SerializeField, Min(0f), Tooltip("ダメージ数値をカメラ側へ寄せる距離")]
        private float _damageNumberCameraOffset = 0.2f;

        private IHealthHudPresenter _presenter;
        private DamageNumberPoolView _damageNumberPoolView;

        /// <summary>
        ///     下半円上からランダムなダメージ数値の表示位置を取得する。
        /// </summary>
        /// <returns> ダメージ数値の表示位置。 </returns>
        private Vector3 GetDamageNumberPos()
        {
            UnityEngine.Camera targetCamera = UnityEngine.Camera.main;
            Vector3 headPosition = _damageNumberSpawnPoint.position;

            if (targetCamera == null)
            {
                return headPosition;
            }

            Vector3 cameraUp = targetCamera.transform.up;
            Vector3 cameraRight = targetCamera.transform.right;

            // 敵の頭から画面上方向へずらした位置を円の中心にする。
            Vector3 circleCenter = headPosition + cameraUp * _damageNumberCircleCenterOffset;

            // 円の下半分の半円上にランダム配置する。
            float angle = Random.Range(0f, 180f) * Mathf.Deg2Rad;

            Vector3 offset = cameraRight * Mathf.Cos(angle) + cameraUp * Mathf.Sin(angle);

            Vector3 position = circleCenter + offset * _damageNumberCircleRadius;

            return GetCameraOffsetPosition(position, targetCamera);
        }

        /// <summary>
        ///     ダメージ数値をプレイヤー視点へ向ける回転を取得する。
        /// </summary>
        /// <param name="position"> ダメージ数値の表示位置。 </param>
        /// <returns> ダメージ数値の表示回転。 </returns>
        private static Quaternion GetDamageNumberRotation(Vector3 position)
        {
            UnityEngine.Camera targetCamera = UnityEngine.Camera.main;

            if (targetCamera == null)
            {
                return Quaternion.identity;
            }

            Vector3 lookDirection = position - targetCamera.transform.position;

            if (lookDirection.sqrMagnitude <= Mathf.Epsilon)
            {
                return Quaternion.identity;
            }

            return Quaternion.LookRotation(lookDirection.normalized, targetCamera.transform.up);
        }

        /// <summary>
        ///     ダメージ数値をカメラ側へ移動した位置を取得する。
        /// </summary>
        /// <param name="position"> 基準位置。 </param>
        /// <param name="targetCamera"> 対象カメラ。 </param>
        /// <returns> 補正後の位置。 </returns>
        private Vector3 GetCameraOffsetPosition(Vector3 position, UnityEngine.Camera targetCamera)
        {
            if (_damageNumberCameraOffset <= 0f)
            {
                return position;
            }

            Vector3 toCamera = targetCamera.transform.position - position;

            if (toCamera.sqrMagnitude <= Mathf.Epsilon)
            {
                return position;
            }

            return position + toCamera.normalized * _damageNumberCameraOffset;
        }
    }
}