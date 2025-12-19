using UnityEngine;

namespace Mock.MusicBattle.MusicSync
{
    /// <summary>
    ///     ビートインジケーターを制御するクラス（開発用）。
    /// </summary>
    public class BeatIndicatorController : MonoBehaviour
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
        /// <summary> 音楽アクションハンドラーの参照。 </summary>
        [SerializeField, Tooltip("音楽アクションハンドラーの参照。")]
        private MusicActionHandler _musicActionHandler;
        /// <summary> インジケーターのズーム速度。 </summary>
        [SerializeField, Tooltip("インジケーターのズーム速度。")]
        private float zoomSpeed = 0.016f;
        #endregion

        // PRIVATE_FIELDS
        #region Unityライフサイクルメソッド
        /// <summary>
        ///     最初のフレームアップデートの前に呼び出されます。
        ///     音楽アクションハンドラーのイベントを購読します。
        /// </summary>
        void Start()
        {
            _musicActionHandler.OnBeat += BeatAction;
        }

        /// <summary>
        ///     固定フレームレートで呼び出されます。
        ///     インジケーターのスケールを減少させます。
        /// </summary>
        private void FixedUpdate()
        {
            transform.localScale -= Vector3.one * zoomSpeed;
            if (transform.localScale.x < 0f)
            {
                transform.localScale = Vector3.zero;
            }
        }
        #endregion

        #region イベントハンドラメソッド
        /// <summary>
        ///     音楽のビートに合わせてインジケーターをリセットするアクション。
        /// </summary>
        private void BeatAction()
        {
            transform.localScale = Vector3.one;
        }
        #endregion

        // PROTECTED_INTERFACE_VIRTUAL_METHODS
        // PRIVATE_METHODS
        // PRIVATE_ENUM_DEFINITIONS
        // PRIVATE_CLASS_DEFINITIONS
        // PRIVATE_STRUCT_DEFINITIONS
    }
}
