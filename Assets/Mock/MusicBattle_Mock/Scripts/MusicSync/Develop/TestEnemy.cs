using System;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace Mock.MusicBattle.MusicSync
{
    /// <summary>
    ///     音楽同期システムをテストするための敵の挙動を模倣するクラス（開発用）。
    /// </summary>
    public class TestEnemy : MonoBehaviour
    {
        // PUBLIC_METHODS
        #region インスペクター表示フィールド
        /// <summary> 音楽同期マネージャーの参照。 </summary>
        [SerializeField, Tooltip("音楽同期マネージャーの参照。")]
        private MusicSyncManager _musicSyncManager;

        /// <summary> 小節タイミング。 </summary>
        [Header("デバッグ用"), SerializeField, Tooltip("小節タイミング。")]
        private long _barFlg = 1;
        /// <summary> 拍子スケール。 </summary>
        [SerializeField, Tooltip("拍子スケール。")]
        private long _timeSignature = 4;
        /// <summary> 拍数。 </summary>
        [SerializeField, Tooltip("拍数。")]
        private long _targetBeat = 0;
        /// <summary> ズーム速度。 </summary>
        [SerializeField, Tooltip("ズーム速度。")]
        private float _zoomSpeed = 0.02f;
        /// <summary> 表示する画像。 </summary>
        [SerializeField, Tooltip("表示する画像。")]
        private Image _image;
        #endregion

        #region プライベートフィールド
        /// <summary> アクションキャンセル用のCancellationTokenSource。 </summary>
        private CancellationTokenSource _cancellationTokenSource;
        #endregion

        #region Unityライフサイクルメソッド
        /// <summary>
        ///     最初のフレームアップデートの前に呼び出されます。
        /// </summary>
        void Start()
        {
        }

        /// <summary>
        ///     フレームごとに呼び出されます。
        ///     ズームエフェクトと入力によるアクションの登録・キャンセルを処理します。
        /// </summary>
        void Update()
        {
            if(transform.localScale.x > 1f)
            {
                transform.localScale -= Vector3.one * _zoomSpeed;
            }
            if (Input.GetKeyDown(KeyCode.Q))
            {
                RegisterAction();
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                CancelAction();
            }
        }
        #endregion
        #region Privateメソッド
        /// <summary>
        ///     音楽同期アクションを登録します。
        /// </summary>
        private void RegisterAction()
        {
            BarTimingInfo barTimingInfo = new BarTimingInfo(_barFlg, _timeSignature, _targetBeat);
            _cancellationTokenSource = new CancellationTokenSource();
            _musicSyncManager.RegisterAction(barTimingInfo, () => TestScheduledAction(_cancellationTokenSource.Token),
                _cancellationTokenSource.Token);

            transform.localScale = Vector3.one * 0.75f;
            _image.color = Color.red;
        }

        /// <summary>
        ///     テスト用の予約されたアクション。
        /// </summary>
        /// <param name="token">キャンセル用のCancellationToken。</param>
        private void TestScheduledAction(CancellationToken token)
        {
            if(token.IsCancellationRequested)
            {
                Debug.Log("敵側：予約アクションはキャンセルされました。何もしません。");
                return;
            }
            Debug.Log("敵側：アクションが発火しました。");
            transform.localScale = Vector3.one * 1.5f;
            _image.color = Color.white;
        }

        /// <summary>
        ///     予約されたアクションをキャンセルします。
        /// </summary>
        private void CancelAction()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                transform.localScale = Vector3.one;
                _image.color = Color.white;
            }
        }
        #endregion
    }
}

