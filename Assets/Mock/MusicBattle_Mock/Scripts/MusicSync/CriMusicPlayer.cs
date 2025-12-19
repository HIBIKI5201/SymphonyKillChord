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
        // CONSTRUCTOR
        // PUBLIC_EVENTS
        #region パブリックプロパティ
        /// <summary> CRI Atom Sourceコンポーネント。 </summary>
        public CriAtomSource Source => _audioSource;
        #endregion

        // INTERFACE_PROPERTIES
        // PUBLIC_CONSTANTS
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

        // PUBLIC_INTERFACE_METHODS
        // PUBLIC_ENUM_DEFINITIONS
        // PUBLIC_CLASS_DEFINITIONS
        // PUBLIC_STRUCT_DEFINITIONS
        // CONSTANTS
        // INSPECTOR_FIELDS
        #region プライベートフィールド
        /// <summary> 音源となるCriAtomSourceコンポーネント。 </summary>
        private CriAtomSource _audioSource;
        #endregion

        // UNITY_LIFECYCLE_METHODS
        // EVENT_HANDLER_METHODS
        // PROTECTED_INTERFACE_VIRTUAL_METHODS
        // PRIVATE_METHODS
        // PRIVATE_ENUM_DEFINITIONS
        // PRIVATE_CLASS_DEFINITIONS
        // PRIVATE_STRUCT_DEFINITIONS
    }
}