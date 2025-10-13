using CriWare;
using UnityEngine;

namespace Mock.MusicSyncMock
{
    /// <summary>
    ///     Criの音楽を再生するクラス。
    /// </summary>
    public class CriMusicPlayer : MonoBehaviour
    {
        [SerializeField]
        private CriAtomSource _audioSource;
        [SerializeField]
        private long _bpm = 120L;

        private CriMusicBuffer _musicBuffer;

        private void Awake()
        {
            _musicBuffer = GetComponent<CriMusicBuffer>();
        }

        private void Start()
        {
            _musicBuffer.Play(_audioSource, _bpm);
        }
    }
}