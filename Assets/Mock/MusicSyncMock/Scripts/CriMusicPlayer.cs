using CriWare;
using UnityEngine;

namespace Mock.MusicSyncMock
{
    /// <summary>
    ///     Criの音楽を再生するクラス。
    /// </summary>
    public class CriMusicPlayer : MonoBehaviour
    {
        private CriAtomSource _audioSource;

        /// <summary>CriAtomSourceコンポーネント</summary>
        public CriAtomSource Source => _audioSource;

        #region ライフサイクル
        #endregion

        /// <summary>
        /// 初期化を行う。
        /// </summary>
        /// <param name="source"></param>
        public void Init(CriAtomSource source)
        {
            _audioSource = source;
        }
        /// <summary>
        /// BGM再生を開始する。
        /// </summary>
        public void Play()
        {
            _audioSource.Play();
        }
    }
}