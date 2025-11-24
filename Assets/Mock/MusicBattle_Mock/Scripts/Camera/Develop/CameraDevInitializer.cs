using Mock.MusicBattle.Basis;
using Mock.MusicBattle.Battle;
using Mock.MusicBattle.Camera;
using UnityEngine;

namespace Mock.MusicBattle.Develop
{
    public class CameraDevInitializer : MonoBehaviour
    {
        [SerializeField]
        private CameraManager _cameraManager;
        [SerializeField]
        private LockOnTargetContainerForCamera _targetContainer;
        [SerializeField]
        private InputBuffer _inputBuffer;

        void Start()
        {
            LockOnManager lockOnManager = new(_cameraManager.transform, _targetContainer, _inputBuffer);

            bool isSuccess = true;
            isSuccess = isSuccess && _cameraManager.Init(_inputBuffer, lockOnManager);

            Debug.Log(isSuccess ? "初期化は正常に終了した。" : "初期化は失敗した。");
        }
    }
}
