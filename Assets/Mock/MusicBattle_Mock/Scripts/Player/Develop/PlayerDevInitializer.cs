using Mock.MusicBattle.Basis;
using Mock.MusicBattle.Camera;
using UnityEngine;

namespace Mock.MusicBattle.Develop
{
    public class PlayerDevInitializer : MonoBehaviour
    {
        [SerializeField] private PlayerManager _playerManager;
        [SerializeField] private InputBuffer _inputBuffer;
        [SerializeField] private CameraManager _cameraManager;
        [SerializeField] private LockOnTargetContainerForCamera _targetContainer;

        void Awake()
        {
            bool isSuccess = true;
            isSuccess = isSuccess && _cameraManager.Init(_inputBuffer, _targetContainer);
            Debug.Log(isSuccess ? "初期化は正常に終了した。" : "初期化は失敗した。");
            _playerManager.Init(_inputBuffer);
        }
    }
}
