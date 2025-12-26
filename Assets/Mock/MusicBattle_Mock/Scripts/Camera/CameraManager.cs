using Mock.MusicBattle.Battle;
using Mock.MusicBattle.Basis;
using System;
using Unity.Cinemachine;
using UnityEngine;

namespace Mock.MusicBattle.Camera
{
    /// <summary>
    ///     カメラのマネージャークラス。
    ///     カメラの各モジュールを実行する。
    /// </summary>
    [RequireComponent(typeof(CinemachineCamera))]
    public class CameraManager : MonoBehaviour, IDisposable
    {
        #region Publicメソッド
        /// <summary>
        ///     カメラを初期化します。
        /// </summary>
        /// <param name="inputBuffer">入力バッファ。</param>
        /// <param name="lockOnManager">ロックオンマネージャー。</param>
        /// <returns>初期化が成功した場合はtrue、失敗した場合はfalse。</returns>
        public bool Init(
            InputBuffer inputBuffer,
            LockOnManager lockOnManager)
        {
            #region バリデーションチェック
            if (inputBuffer == null)
            {
                Debug.LogError($"{nameof(InputBuffer)} がnullです。");
                return false;
            }
            if (lockOnManager == null)
            {
                Debug.LogError($"{nameof(ILockOnTargetContainer)} がnullです。");
                return false;
            }
            if (_cameraConfigs == null)
            {
                Debug.LogError($"{nameof(CameraConfigs)} がnullです。");
                return false;
            }
            #endregion

            CinemachineCamera cam = GetComponent<CinemachineCamera>();

            // イベント登録。
            inputBuffer.LookAction.Performed += HandleLookAction;
            inputBuffer.LookAction.Canceled += HandleLookAction;
            lockOnManager.OnTargetLocked += HandleLockOn;

            _mover = new(_cameraConfigs, transform, cam.Follow);

            _lockOnManager = lockOnManager;
            _inputBuffer = inputBuffer;

            return true;
        }

        /// <summary>
        ///     アップデートモードを変更します。
        /// </summary>
        /// <param name="mode">新しいカメラの更新モード。</param>
        public void ChangeUpdateMode(CameraUpdateModeEnum mode)
        {
            _mode = mode;
        }
        #endregion

        #region パブリックインターフェースメソッド
        /// <summary>
        ///     このインスタンスによって使用されているリソースを解放します。
        ///     入力イベントの登録を解除します。
        /// </summary>
        public void Dispose()
        {
            if (_inputBuffer != null)
            {
                _inputBuffer.LookAction.Performed -= HandleLookAction;
                _inputBuffer.LookAction.Canceled -= HandleLookAction;
            }
        }
        #endregion

        #region インスペクター表示フィールド
        /// <summary> カメラの設定データ。 </summary>
        [SerializeField, Tooltip("カメラの設定データ。")]
        private CameraConfigs _cameraConfigs;
        #endregion

        #region プライベートフィールド
        /// <summary> ロックオンマネージャーの参照。 </summary>
        private LockOnManager _lockOnManager;
        /// <summary> 入力バッファの参照。 </summary>
        private InputBuffer _inputBuffer;
        /// <summary> カメラの移動処理。 </summary>
        private CameraMover _mover;
        /// <summary> カメラの更新モード。 </summary>
        private CameraUpdateModeEnum _mode = CameraUpdateModeEnum.Update;
        #endregion

        #region Unityライフサイクルメソッド
        /// <summary>
        ///     フレームごとに呼び出されます。
        ///     現在の更新モードがUpdateの場合にカメラを更新します。
        /// </summary>
        private void Update()
        {
            if (_mode != CameraUpdateModeEnum.Update) { return; }
            Tick(Time.deltaTime);
        }

        /// <summary>
        ///     固定フレームレートで呼び出されます。
        ///     現在の更新モードがFixedUpdateの場合にカメラを更新します。
        /// </summary>
        private void FixedUpdate()
        {
            if (_mode != CameraUpdateModeEnum.FixedUpdate) { return; }
            Tick(Time.fixedDeltaTime);
        }

        /// <summary>
        ///     すべてのUpdate呼び出しの後にフレームごとに呼び出されます。
        ///     現在の更新モードがLateUpdateの場合にカメラを更新します。
        /// </summary>
        private void LateUpdate()
        {
            if (_mode != CameraUpdateModeEnum.LateUpdate) { return; }
            Tick(Time.deltaTime);
        }

        /// <summary>
        ///     シーンビューで選択されたときにギズモを描画します。
        /// </summary>
        private void OnDrawGizmos()
        {
            _mover?.OnDrawGizmos();
        }
        #endregion

        #region イベントハンドラメソッド
        /// <summary>
        ///     Lookアクションの入力時に呼び出されます。
        /// </summary>
        /// <param name="value">入力値。</param>
        private void HandleLookAction(Vector2 value)
        {
            _mover?.RotateCamera(value);
        }

        /// <summary>
        ///     ターゲットがロックオンされたときに呼び出されます。
        /// </summary>
        /// <param name="target">ロックオンされたターゲットのTransform。</param>
        private void HandleLockOn(Transform target) => _mover?.SetLockTarget(target);
        #endregion

        #region Privateメソッド
        /// <summary>
        ///     1フレームごとの更新を行います。
        /// </summary>
        /// <param name="deltaTime">デルタタイム。</param>
        private void Tick(float deltaTime)
        {
            // 移動モジュールを更新。
            _mover?.UpdatePitch(deltaTime);
            _mover?.UpdateYaw(deltaTime);
        }
        #endregion
    }
}