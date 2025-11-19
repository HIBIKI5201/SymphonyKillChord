using Mock.MusicBattle.Basis;
using Mock.MusicBattle.Camera;
using UnityEngine;

namespace Mock.MusicBattle.Develop
{
    public class CameraDevInitializer : MonoBehaviour
    {
        [SerializeField]
        private CameraManager _cameraManager;
        [SerializeField]
        private InputBuffer _inputBuffer;

        void Start()
        {
            _cameraManager.Init(_inputBuffer, null);
        }
    }
}
