using CriWare;
using UnityEngine;

namespace Mock.MusicBattle.MusicSync
{
    /// <summary>
    ///     CRIミドルウェアの機能を利用して音楽を再生するクラス。
    /// </summary>
    [DisallowMultipleComponent]
    public class CriMusicPlayer : MonoBehaviour
    {
        #region パブリックプロパティ
        /// <summary> CRI Atom Sourceコンポーネント。 </summary>
        public CriAtomSource Source => _audioSource;
        #endregion

        #region Publicメソッド
        /// <summary>
        ///     初期化を行います。
        /// </summary>
        /// <param name="source">再生に使用するCriAtomSource。</param>
        public void Init(CriAtomSource source)
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
        private CriAtomSource _audioSource;
        #endregion
    }
}