using UnityEngine;

namespace Mock.MusicSyncMock
{
    public class MusicPlayer : MonoBehaviour
    {
        [SerializeField]
        private AudioSource _audioSource;
        [SerializeField]
        private long _bpm = 120L;

        private MusicBuffer _musicBuffer;

        private void Awake()
        {
            _musicBuffer = GetComponent<MusicBuffer>();
        }

        private void Start()
        {
            _musicBuffer.Play(_audioSource, _bpm);
        }
    }
}
