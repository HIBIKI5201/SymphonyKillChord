using Mock.MusicBattle.Basis;
using Mock.MusicBattle.Battle;
using Mock.MusicBattle.Camera;
using Mock.MusicBattle.Player;
using Unity.Cinemachine;
using UnityEngine;

namespace Mock.MusicBattle.Develop
{
    public class PlayerDevInitializer : MonoBehaviour
    {
        [SerializeField] private PlayerManager _playerManager;
        [SerializeField] private InputBuffer _inputBuffer;
        [SerializeField] private CameraManager _cameraManager;
        [SerializeField] private LockOnTargetContainerForCamera _targetContainer;
        [SerializeField] private CinemachineCamera _camera;

        void Awake()
        {
            LockOnManager lockOnManager = new(_cameraManager.transform, _targetContainer, _inputBuffer);
            bool isSuccess = true;
            isSuccess = isSuccess && _cameraManager.Init(_inputBuffer, lockOnManager);
            Debug.Log(isSuccess ? "初期化は正常に終了した。" : "初期化は失敗した。");
            _playerManager.Init(_inputBuffer, _camera);
        }
    }
}
