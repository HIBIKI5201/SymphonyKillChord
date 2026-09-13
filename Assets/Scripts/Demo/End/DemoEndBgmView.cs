using KillChord.Runtime.Adaptor.Persistent.Music;
using LitMotion;
using System;
using System.Threading;
using UnityEngine;

namespace KillChord.Demo.End
{
    /// <summary>
    ///     体験版終了演出のBGM再生とフェードアウトを行うViewです。
    /// </summary>
    public sealed class DemoEndBgmView : MonoBehaviour
    {
        /// <summary>
        ///     BGM再生と音量制御に使うPersistentのポートを設定します。
        /// </summary>
        /// <param name="cuePlayer"> BGM Cueの切り替えポートです。 </param>
        /// <param name="volumeManager"> BGM音量の制御ポートです。 </param>
        public void Initialize(IBgmCuePlayer cuePlayer, IVolumeManager volumeManager)
        {
            _cuePlayer = cuePlayer ?? throw new ArgumentNullException(nameof(cuePlayer));
            _volumeManager = volumeManager ?? throw new ArgumentNullException(nameof(volumeManager));

            // フェードアウトで書き換える前の音量比率を、復帰用に保持する。
            _baseVolumeRatio = _volumeManager.GetVolume();
        }

        /// <summary>
        ///     指定したBGM Cueを再生します。
        /// </summary>
        /// <param name="cueName"> 再生するCue名です。 </param>
        public void Play(string cueName)
        {
            if (_cuePlayer == null)
            {
                Debug.LogError(
                    $"[{nameof(DemoEndBgmView)}] {nameof(Initialize)} が呼ばれていません。",
                    this);
                return;
            }

            if (string.IsNullOrWhiteSpace(cueName))
            {
                Debug.LogError(
                    $"[{nameof(DemoEndBgmView)}] BGMのCue名が設定されていません。",
                    this);
                return;
            }

            CancelFade();
            _volumeManager?.SetVolume(_baseVolumeRatio);
            _cuePlayer.SetCue(cueName);
        }

        /// <summary>
        ///     BGMをフェードアウトさせ、完了するまで待機します。
        /// </summary>
        /// <param name="duration"> フェードアウトにかける秒数です。 </param>
        /// <param name="cancellationToken"> キャンセル用のトークンです。 </param>
        /// <returns> フェードアウト完了まで待機するAwaitableです。 </returns>
        public async Awaitable FadeOutAsync(
            float duration,
            CancellationToken cancellationToken)
        {
            if (_volumeManager == null)
            {
                Debug.LogError(
                    $"[{nameof(DemoEndBgmView)}] {nameof(Initialize)} が呼ばれていません。",
                    this);
                return;
            }

            if (duration <= 0.0f)
            {
                _volumeManager.SetVolume(SILENT_VOLUME_RATIO);
                return;
            }

            CancelFade();

            bool isCompleted = false;
            IVolumeManager volumeManager = _volumeManager;
            _fadeHandle = LMotion.Create(_baseVolumeRatio, SILENT_VOLUME_RATIO, duration)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .WithOnComplete(() => isCompleted = true)
                .Bind(volume => volumeManager.SetVolume(volume))
                .AddTo(gameObject);

            try
            {
                while (!isCompleted)
                {
                    await Awaitable.NextFrameAsync(cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                CancelFade();
                throw;
            }

            _volumeManager.SetVolume(SILENT_VOLUME_RATIO);
        }

        /// <summary>
        ///     BGMを停止し、音量比率を演出開始前の値へ戻します。
        /// </summary>
        public void StopAndRestoreVolume()
        {
            CancelFade();
            _cuePlayer?.SetCue(string.Empty);
            _volumeManager?.SetVolume(_baseVolumeRatio);
        }

        private const float SILENT_VOLUME_RATIO = 0.0f;

        private IBgmCuePlayer _cuePlayer;
        private IVolumeManager _volumeManager;
        private MotionHandle _fadeHandle;
        private float _baseVolumeRatio = 1.0f;

        /// <summary>
        ///     破棄時に再生中のフェードを停止します。
        /// </summary>
        private void OnDestroy()
        {
            CancelFade();
        }

        /// <summary>
        ///     再生中のフェードモーションを停止します。
        /// </summary>
        private void CancelFade()
        {
            _fadeHandle.TryCancel();
        }
    }
}
