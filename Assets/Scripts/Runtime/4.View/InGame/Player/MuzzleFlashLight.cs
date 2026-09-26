using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace KillChord.Runtime.View
{
    /// <summary>
    ///     射撃時にマズルフラッシュのライトを一瞬点灯させる。
    /// </summary>
    [RequireComponent(typeof(Light))]
    public class MuzzleFlashLight : MonoBehaviour
    {
        /// <summary> ライトが現在点灯しているかを取得します。 </summary>
        public bool IsFlashing => _light != null && _light.enabled;

        /// <summary>
        ///     非同期待機やキャンセルトークンを作らず、設定時間だけ点灯します。
        /// </summary>
        public void Play()
        {
            _timedFlashEndTime = Time.time + _duration;
            _isTimedFlashActive = true;
            _light.enabled = true;
        }

        /// <summary>
        ///     待機中も同じ実体を再利用できるよう、ライトと消灯予約だけを停止します。
        /// </summary>
        public void Stop()
        {
            _isTimedFlashActive = false;
            if (_light != null)
            {
                _light.enabled = false;
            }
        }

        /// <summary>
        ///     ライトを点灯し、設定時間が経ったら消灯する。
        /// </summary>
        public async ValueTask Flash(CancellationToken token = default)
        {
            _light.enabled = true;
            await Awaitable.WaitForSecondsAsync(_duration, token);
            _light.enabled = false;
        }

        [SerializeField, Tooltip("フラッシュの持続時間")]
        private float _duration = 0.1f;

        private Light _light;
        private float _timedFlashEndTime;
        private bool _isTimedFlashActive;

        /// <summary>
        ///     Light コンポーネントを取得する。
        /// </summary>
        private void Awake()
        {
            _light = GetComponent<Light>();
        }

        /// <summary>
        ///     有効化時にライトを消灯する。
        /// </summary>
        private void OnEnable()
        {
            Stop();
        }

        /// <summary>
        ///     無効化時にライトを消灯する。
        /// </summary>
        private void OnDisable()
        {
            Stop();
        }

        /// <summary>
        ///     設定した期限に達した発射ライトを消灯します。
        /// </summary>
        private void Update()
        {
            if (_isTimedFlashActive && Time.time >= _timedFlashEndTime)
            {
                Stop();
            }
        }
    }
}
