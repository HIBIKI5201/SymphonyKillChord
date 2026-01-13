using CriWare;
using UnityEngine;

namespace Mock.MusicBattle.MusicSync
{
    /// <summary>
    ///     CRIミドルウェアの機能を利用して音楽を再生するクラス。
    /// </summary>
    public class CriMusicPlayer
    {
        #region パブリックプロパティ
        /// <summary> CRI Atom Sourceコンポーネント。 </summary>
        public CriAtomSource Source => _audioSource;
        public double Time => _audioSource.time;
        #endregion

        #region Publicメソッド
        /// <summary>
        ///     初期化を行います。
        /// </summary>
        /// <param name="source">再生に使用するCriAtomSource。</param>
        public CriMusicPlayer(CriAtomSource source)
        {
            _audioSource = source;
        }

        /// <summary>
        ///     BGMの再生を開始します。
        /// </summary>
        public void Play()
        {
            _audioSource.Play();
        }
        #endregion

        #region プライベートフィールド
        /// <summary> 音源となるCriAtomSourceコンポーネント。 </summary>
        private readonly CriAtomSource _audioSource;
        #endregion
    }
}
