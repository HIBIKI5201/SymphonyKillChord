using System;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace Mock.MusicSyncMock
{
    public class TestEnemy : MonoBehaviour
    {
        [SerializeField] private MusicSyncManager _musicSyncManager;

        [Header("デバッグ用")]
        [SerializeField, Tooltip("小節タイミング")] private long _barFlg = 1;
        [SerializeField, Tooltip("拍子スケール")] private long _timeSignature = 4;
        [SerializeField, Tooltip("拍数")] private long _targetBeat = 0;
        [SerializeField,] private float _zoomSpeed = 0.02f;
        [SerializeField] private Image _image;

        private CancellationTokenSource _cancellationTokenSource;
        void Start()
        {
        }

        // Update is called once per frame
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

        private void RegisterAction()
        {
            StringBuilder debugLog = new StringBuilder();
            BarTimingInfo barTimingInfo = new BarTimingInfo(_barFlg, _timeSignature, _targetBeat);
            _cancellationTokenSource = new CancellationTokenSource();
            _musicSyncManager.RegisterAction(barTimingInfo, TestScheduledAction, _cancellationTokenSource.Token);

            transform.localScale = Vector3.one * 0.75f;
            _image.color = Color.red;
        }

        private void TestScheduledAction()
        {
            transform.localScale = Vector3.one * 1.5f;
            _image.color = Color.white;
        }

        private void CancelAction()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                transform.localScale = Vector3.one;
                _image.color = Color.white;
            }
        }
    }
}
