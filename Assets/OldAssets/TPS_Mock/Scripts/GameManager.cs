using UnityEngine;

namespace Mock.TPS
{
    /// <summary>
    ///     ゲーム全体の管理クラス。
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [SerializeField]
        private InputBuffer _inputBuffer;

        [SerializeField]
        private CameraManager _cameraManager;
        [SerializeField]
        private PlayerManager _playerManager;

        private void Awake()
        {
            _cameraManager.Init(_inputBuffer);
            _playerManager.Init(_inputBuffer, _cameraManager);
        }
    }
}