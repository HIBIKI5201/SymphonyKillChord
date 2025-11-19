using Mock.MusicBattle.Camera;
using Mock.MusicBattle.Develop;
using UnityEngine;

namespace Mock.MusicBattle.Develop
{
    public class CameraDevInitializer : MonoBehaviour
    {
        [SerializeField]
        private CameraManager _cameraManager;
        [SerializeField]
        private CameraInputBuffer _inputBuffer;

        void Start()
        {
            _cameraManager.Init(_inputBuffer, null);
        }
    }
}
