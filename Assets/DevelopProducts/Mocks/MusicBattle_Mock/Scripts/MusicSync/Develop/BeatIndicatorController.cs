using UnityEngine;

namespace Mock.MusicBattle.MusicSync
{
    /// <summary>
    ///     ビートインジケーターを制御するクラス（開発用）。
    /// </summary>
    public class BeatIndicatorController : MonoBehaviour
    {

        #region インスペクター表示フィールド
        /// <summary> 音楽アクションハンドラーの参照。 </summary>
        [SerializeField, Tooltip("音楽アクションハンドラーの参照。")]
        private MusicActionHandler _musicActionHandler;
        /// <summary> インジケーターのズーム速度。 </summary>
        [SerializeField, Tooltip("インジケーターのズーム速度。")]
        private float _zoomSpeed = 0.016f;
        #endregion
        #region Unityライフサイクルメソッド
        /// <summary>
        ///     最初のフレームアップデートの前に呼び出されます。
        ///     音楽アクションハンドラーのイベントを購読します。
        /// </summary>
        private void Start()
        {
            _musicActionHandler.OnBeat += BeatAction;
        }

        /// <summary>
        ///     固定フレームレートで呼び出されます。
        ///     インジケーターのスケールを減少させます。
        /// </summary>
        private void FixedUpdate()
        {
            transform.localScale -= Vector3.one * _zoomSpeed;
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
    }
}

