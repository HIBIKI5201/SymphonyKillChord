using Mock.MusicBattle.Basis;
using Mock.MusicBattle.Battle;
using Mock.MusicBattle.Camera;
using Mock.MusicBattle.Player;
using Unity.Cinemachine;
using UnityEngine;

namespace Mock.MusicBattle.Develop
{
    /// <summary>
    ///     プレイヤーの開発用初期化を行うクラス。
    /// </summary>
    public class PlayerDevInitializer : MonoBehaviour
    {
        #region インスペクター表示フィールド
        /// <summary> プレイヤーマネージャーの参照。 </summary>
        [SerializeField, Tooltip("プレイヤーマネージャーの参照。")]
        private PlayerManager _playerManager;
        /// <summary> 入力バッファの参照。 </summary>
        [SerializeField, Tooltip("入力バッファの参照。")]
        private InputBuffer _inputBuffer;
        /// <summary> カメラマネージャーの参照。 </summary>
        [SerializeField, Tooltip("カメラマネージャーの参照。")]
        private CameraManager _cameraManager;
        /// <summary> ロックオンターゲットコンテナの参照。 </summary>
        [SerializeField, Tooltip("ロックオンターゲットコンテナの参照。")]
        private LockOnTargetContainerForCamera _targetContainer;
        /// <summary> Cinemachineカメラの参照。 </summary>
        [SerializeField, Tooltip("Cinemachineカメラの参照。")]
        private CinemachineCamera _camera;
        #endregion

        // PRIVATE_FIELDS
        #region Unityライフサイクルメソッド
        /// <summary>
        ///     スクリプトインスタンスがロードされたときに呼び出されます。
        ///     プレイヤーとカメラの初期化を行います。
        /// </summary>
        void Awake()
        {
            LockOnManager lockOnManager = new(_cameraManager.transform, _targetContainer, _inputBuffer);
            bool isSuccess = true;
            isSuccess = isSuccess && _cameraManager.Init(_inputBuffer, lockOnManager);
            Debug.Log(isSuccess ? "初期化は正常に終了しました。" : "初期化は失敗しました。");
            // デバッグ用のためMusicSyncManagerはnullを渡す。
            _playerManager.Init(_inputBuffer, _camera, lockOnManager, null); 
        }
        #endregion

        // PRIVATE_METHODS
    }
}

