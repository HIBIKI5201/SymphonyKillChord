using Mock.MusicBattle.Basis;
using Mock.MusicBattle.Battle;
using Mock.MusicBattle.Camera;
using UnityEngine;

namespace Mock.MusicBattle.Develop
{
    /// <summary>
    ///     カメラの開発用初期化を行うクラス。
    /// </summary>
    public class CameraDevInitializer : MonoBehaviour
    {
        #region インスペクター表示フィールド
        /// <summary> カメラマネージャーの参照。 </summary>
        [SerializeField, Tooltip("カメラマネージャーの参照。")]
        private CameraManager _cameraManager;
        /// <summary> ロックオンターゲットコンテナの参照。 </summary>
        [SerializeField, Tooltip("ロックオンターゲットコンテナの参照。")]
        private LockOnTargetContainerForCamera _targetContainer;
        /// <summary> 入力バッファの参照。 </summary>
        [SerializeField, Tooltip("入力バッファの参照。")]
        private InputBuffer _inputBuffer;
        #endregion

        #region Unityライフサイクルメソッド
        /// <summary>
        ///     最初のフレームアップデートの前に呼び出されます。
        ///     カメラの初期化を行います。
        /// </summary>
        private void Start()
        {
            LockOnManager lockOnManager = new(_cameraManager.transform, _targetContainer, _inputBuffer);

            bool isSuccess = true;
            isSuccess = isSuccess && _cameraManager.Init(_inputBuffer, lockOnManager);

            Debug.Log(isSuccess ? "初期化は正常に終了しました。" : "初期化は失敗しました。");
        }
        #endregion
    }
}

