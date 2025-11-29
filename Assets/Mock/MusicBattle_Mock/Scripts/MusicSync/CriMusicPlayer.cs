using CriWare;
using UnityEngine;

namespace Mock.MusicBattle.MusicSync
{
    /// <summary>
    ///     Criの音楽を再生するクラス。
    /// </summary>
    [DisallowMultipleComponent]
    public class CriMusicPlayer : MonoBehaviour
    {
        #region Publicメソッド
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
        #endregion

        private CriAtomSource _audioSource;

        /// <summary>CriAtomSourceコンポーネント</summary>
        public CriAtomSource Source => _audioSource;

        #region ライフサイクル
        #endregion
    }
}