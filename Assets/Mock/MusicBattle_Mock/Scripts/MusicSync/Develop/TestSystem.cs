using CriWare;
using UnityEngine;

namespace Mock.MusicBattle.MusicSync
{
    /// <summary>
    ///     音楽同期システムのテスト用初期化を行うクラス（開発用）。
    /// </summary>
    public class TestSystem : MonoBehaviour
    {
        // CONSTRUCTOR
        // PUBLIC_EVENTS
        // PUBLIC_PROPERTIES
        // INTERFACE_PROPERTIES
        // PUBLIC_CONSTANTS
        // PUBLIC_METHODS
        // PUBLIC_INTERFACE_METHODS
        // PUBLIC_ENUM_DEFINITIONS
        // PUBLIC_CLASS_DEFINITIONS
        // PUBLIC_STRUCT_DEFINITIONS
        // CONSTANTS
        #region インスペクター表示フィールド
        /// <summary> 音楽同期マネージャー。 </summary>
        [Tooltip("音楽同期マネージャー。"), SerializeField]
        private MusicSyncManager _musicSyncManager;
        /// <summary> CRI Atom Source。 </summary>
        [Tooltip("CRI Atom Source。"), SerializeField]
        private CriAtomSource _source;
        /// <summary> BGMのBPM。 </summary>
        [Tooltip("BGMのBPM。"), SerializeField]
        private double _bpm;
        /// <summary> BGMの固有拍子。 </summary>
        [Tooltip("BGMの固有拍子。"), SerializeField]
        private double _bgmProperTime;
        /// <summary> 再生開始オフセット（ミリ秒）。 </summary>
        [Tooltip("再生開始オフセット（ミリ秒）。"), SerializeField]
        private long _startOffset;
        #endregion

        // PRIVATE_FIELDS
        #region Unityライフサイクルメソッド
        /// <summary>
        ///     最初のフレームアップデートの前に呼び出されます。
        ///     音楽同期マネージャーの初期化を行います。
        /// </summary>
        void Start()
        {
            _musicSyncManager.Init(_source, _bpm, _bgmProperTime, _startOffset);
        }
        #endregion

        // EVENT_HANDLER_METHODS
        // PROTECTED_INTERFACE_VIRTUAL_METHODS
        // PRIVATE_METHODS
        // PRIVATE_ENUM_DEFINITIONS
        // PRIVATE_CLASS_DEFINITIONS
        // PRIVATE_STRUCT_DEFINITIONS
    }
}
