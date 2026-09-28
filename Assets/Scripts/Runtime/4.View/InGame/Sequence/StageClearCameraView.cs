using UnityEngine;

namespace KillChord.Runtime.View.InGame.Sequence
{
    /// <summary>
    ///     クリアTimelineの合図で、プレイヤー正面へカメラを移動します。
    /// </summary>
    public sealed class StageClearCameraView : MonoBehaviour
    {
        /// <summary>
        ///     ステージに生成されたプレイヤーを設定します。
        /// </summary>
        public void Initialize(Transform player)
        {
            _player = player;
        }

        /// <summary>
        ///     TimelineのSignalから、指定秒数で正面カメラへ切り替えます。
        /// </summary>
        public void Play(float duration)
        {
            UnityEngine.Camera mainCamera = UnityEngine.Camera.main;
            if (_player == null || _virtualCamera == null || mainCamera == null)
            {
                Debug.LogError($"[{nameof(StageClearCameraView)}] プレイヤーまたはカメラが未設定です。", this);
                return;
            }

            if (!_ownsCamera)
            {
                _startPosition = mainCamera.transform.position;
                _startRotation = mainCamera.transform.rotation;
                _ownsCamera = true;
            }

            _endPosition = _player.position + _player.forward * _distance + Vector3.up * _height;
            _endRotation = CalculateFramingRotation(mainCamera);
            _duration = Mathf.Max(0f, duration);
            _elapsed = 0f;
            ApplyCamera();
            _virtualCamera.gameObject.SetActive(true);
        }

        /// <summary>
        ///     自分が取得したカメラの外部制御を解放します。
        /// </summary>
        public void Release()
        {
            if (!_ownsCamera)
            {
                return;
            }

            _ownsCamera = false;
            if (_virtualCamera != null)
            {
                _virtualCamera.gameObject.SetActive(false);
            }
        }

        [SerializeField, Tooltip("優先度を戦闘カメラより高く設定したクリア専用CinemachineCamera。")]
        private Transform _virtualCamera;

        [SerializeField, Min(0.1f), Tooltip("プレイヤー正面からの距離。")]
        private float _distance = 3f;

        [SerializeField, Tooltip("カメラの高さ。")]
        private float _height = 1.5f;

        [SerializeField, Tooltip("プレイヤー上の注視点の高さ。")]
        private float _lookAtHeight = 1.25f;

        [SerializeField, Range(0f, 1f), Tooltip("画面内でプレイヤーを配置する水平位置。0が左端、1が右端です。")]
        private float _screenPositionX = 0.25f;

        private Transform _player;
        private Vector3 _startPosition;
        private Vector3 _endPosition;
        private Quaternion _startRotation;
        private Quaternion _endRotation;
        private float _duration;
        private float _elapsed;
        private bool _ownsCamera;

        /// <summary>
        ///     ゲーム停止中も演出を進め、リザルト表示中は正面の構図を保持します。
        /// </summary>
        private void LateUpdate()
        {
            if (_ownsCamera && _virtualCamera != null)
            {
                _elapsed += Time.unscaledDeltaTime;
                ApplyCamera();
            }
        }

        /// <summary>
        ///     シーン遷移やキャンセル時にカメラを解放します。
        /// </summary>
        private void OnDisable()
        {
            Release();
        }

        /// <summary>
        ///     Timelineから指定された時間に合わせてカメラの位置と向きを補間します。
        /// </summary>
        private void ApplyCamera()
        {
            float progress = _duration > 0f ? Mathf.Clamp01(_elapsed / _duration) : 1f;
            progress = Mathf.SmoothStep(0f, 1f, progress);
            _virtualCamera.SetPositionAndRotation(
                Vector3.Lerp(_startPosition, _endPosition, progress),
                Quaternion.Slerp(_startRotation, _endRotation, progress));
        }

        /// <summary>
        ///     プレイヤーが指定した画面内の水平位置に収まるカメラ回転を算出します。
        /// </summary>
        /// <param name="camera"> 画角とアスペクト比を参照するメインカメラ。 </param>
        /// <returns> クリア演出の最終カメラ回転。 </returns>
        private Quaternion CalculateFramingRotation(UnityEngine.Camera camera)
        {
            Vector3 lookAtPosition = _player.position + Vector3.up * _lookAtHeight;
            Quaternion centeredRotation = Quaternion.LookRotation(lookAtPosition - _endPosition, Vector3.up);
            float normalizedScreenOffset = _screenPositionX * 2f - 1f;
            float verticalHalfFieldOfView = camera.fieldOfView * 0.5f * Mathf.Deg2Rad;
            float horizontalOffsetAngle = Mathf.Atan(
                normalizedScreenOffset * Mathf.Tan(verticalHalfFieldOfView) * camera.aspect) * Mathf.Rad2Deg;

            return centeredRotation * Quaternion.Euler(0f, -horizontalOffsetAngle, 0f);
        }
    }
}
