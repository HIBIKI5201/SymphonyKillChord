using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UIElements;
using UnityEngine.Video;

namespace KillChord.Runtime.View.OutGame.Title
{
    /// <summary>
    ///     タイトルの操作待ち中に背景だけをPVへ切り替える演出を所有します。
    /// </summary>
    public sealed class TitleIdleVideoView : MonoBehaviour
    {
        /// <summary>
        ///     前面UIに触れず、背景の動画表示とBGM演出の接続を初期化します。
        /// </summary>
        public bool Initialize(VisualElement titleRoot, Func<bool> canPlay, Action<float> setBgmGain,
            Func<float> getBgmVolume, string movieUrl)
        {
            VisualElement background = titleRoot.Q<VisualElement>("BackGround");
            if (background == null)
            {
                Debug.LogError($"[{nameof(TitleIdleVideoView)}] タイトル背景が見つかりません。", this);
                return false;
            }

            _canPlay = canPlay;
            _setBgmGain = setBgmGain;
            _getBgmVolume = getBgmVolume;
            _layer = new VisualElement { name = "IdleVideoLayer", pickingMode = PickingMode.Ignore };
            _videoMatte = new Image { name = "IdleVideoMatte", pickingMode = PickingMode.Ignore };
            _videoSurface = new Image { name = "IdleVideo", pickingMode = PickingMode.Ignore, scaleMode = ScaleMode.ScaleToFit };
            _blackout = new VisualElement { name = "IdleVideoBlackout", pickingMode = PickingMode.Ignore };
            FillParent(_layer);
            FillParent(_videoMatte);
            FillParent(_videoSurface);
            FillParent(_blackout);
            _videoMatte.style.backgroundColor = Color.black;
            _videoMatte.style.display = DisplayStyle.None;
            _blackout.style.backgroundColor = Color.black;
            _layer.Add(_videoMatte);
            _layer.Add(_videoSurface);
            _layer.Add(_blackout);
            VisualElement particleLayer = background.Q<VisualElement>("ParticleLayer");
            background.Insert(particleLayer != null ? background.IndexOf(particleLayer) + 1 : 0, _layer);
            _layer.style.display = DisplayStyle.None;

            _player = gameObject.AddComponent<VideoPlayer>();
            _player.playOnAwake = false;
            _player.isLooping = false;
            _player.waitForFirstFrame = true;
            _player.source = VideoSource.Url;
            _player.url = movieUrl;
            _player.audioOutputMode = VideoAudioOutputMode.Direct;
            _player.controlledAudioTrackCount = 1;
            _player.EnableAudioTrack(VIDEO_AUDIO_TRACK, true);
            _player.SetDirectAudioMute(VIDEO_AUDIO_TRACK, true);
            _player.renderMode = VideoRenderMode.RenderTexture;
            _player.loopPointReached += PlaybackEndedHandler;
            _player.errorReceived += PlaybackErrorHandler;
            _player.frameReady += FrameReadyHandler;
            _lastInputTime = Time.unscaledTime;
            _isInitialized = true;
            return true;
        }

        /// <summary>
        ///     遅延処理を無効化してBGMと背景を戻し、動画用リソースを解放します。
        /// </summary>
        public void Shutdown()
        {
            if (_isDisposed)
            {
                return;
            }
            _isDisposed = true;
            CancelCycle();
            if (_player != null)
            {
                _player.loopPointReached -= PlaybackEndedHandler;
                _player.errorReceived -= PlaybackErrorHandler;
                _player.frameReady -= FrameReadyHandler;
                _player.targetTexture = null;
            }
            _layer?.RemoveFromHierarchy();
            if (_renderTexture != null)
            {
                _renderTexture.Release();
                Destroy(_renderTexture);
                _renderTexture = null;
            }
            _canPlay = null;
            _setBgmGain = null;
            _getBgmVolume = null;
        }

        private const float IDLE_SECONDS = 90f;
        private const float FADE_SECONDS = 2f;
        private const float PREPARE_TIMEOUT_SECONDS = 30f;
        private const int VIDEO_WIDTH = 1920;
        private const int VIDEO_HEIGHT = 1080;
        private const ushort VIDEO_AUDIO_TRACK = 0;
        private Func<bool> _canPlay;
        private Action<float> _setBgmGain;
        private Func<float> _getBgmVolume;
        private VisualElement _layer;
        private Image _videoMatte;
        private Image _videoSurface;
        private VisualElement _blackout;
        private VideoPlayer _player;
        private RenderTexture _renderTexture;
        private CancellationTokenSource _cycleCancellation;
        private int _generation;
        private float _lastInputTime;
        private float _videoAudioGain;
        private string _playbackError;
        private bool _hasPlaybackEnded;
        private bool _hasFirstFrame;
        private bool _hasBgmOverride;
        private bool _canControlVideoAudio;
        private bool _isInitialized;
        private bool _isDisposed;

        /// <summary>
        ///     操作可能なタイトルだけで無操作時間を数え、入力は既存UIへそのまま渡します。
        /// </summary>
        private void Update()
        {
            if (!_isInitialized || _isDisposed)
            {
                return;
            }
            if (!UnityEngine.Application.isFocused || !_canPlay() || HasInput())
            {
                _lastInputTime = Time.unscaledTime;
                CancelCycle();
                return;
            }
            if (_cycleCancellation == null && Time.unscaledTime - _lastInputTime >= IDLE_SECONDS)
            {
                _cycleCancellation = new CancellationTokenSource();
                PlayCycleAsync(++_generation, _cycleCancellation.Token).Forget();
            }
            ApplyVideoAudioVolume();
        }

        /// <summary>
        ///     フォーカスを失った時間を無操作時間へ含めません。
        /// </summary>
        private void OnApplicationFocus(bool hasFocus)
        {
            _lastInputTime = Time.unscaledTime;
            if (!hasFocus)
            {
                CancelCycle();
            }
        }

        /// <summary>
        ///     無効化時に進行中の演出を止めます。
        /// </summary>
        private void OnDisable()
        {
            CancelCycle();
            _lastInputTime = Time.unscaledTime;
        }

        /// <summary>
        ///     所有者破棄時にも動画と音量演出を回収します。
        /// </summary>
        private void OnDestroy()
        {
            Shutdown();
        }

        /// <summary>
        ///     自然再生終了を記録します。
        /// </summary>
        private void PlaybackEndedHandler(VideoPlayer player)
        {
            _hasPlaybackEnded = true;
        }

        /// <summary>
        ///     コーデックや読込エラーを待機処理へ伝えます。
        /// </summary>
        private void PlaybackErrorHandler(VideoPlayer player, string message)
        {
            _playbackError = message;
        }

        /// <summary>
        ///     最初の描画フレームを確認してから動画を見せます。
        /// </summary>
        private void FrameReadyHandler(VideoPlayer player, long frameIndex)
        {
            _hasFirstFrame = true;
            player.sendFrameReadyEvents = false;
        }

        /// <summary>
        ///     準備、暗転、PV再生、静止背景への復帰を順に実行します。
        /// </summary>
        private async UniTaskVoid PlayCycleAsync(int generation, CancellationToken token)
        {
            try
            {
                if (_renderTexture == null)
                {
                    _renderTexture = new RenderTexture(VIDEO_WIDTH, VIDEO_HEIGHT, 0);
                    if (!_renderTexture.Create())
                    {
                        Destroy(_renderTexture);
                        _renderTexture = null;
                        throw new InvalidOperationException("PVの描画テクスチャを作成できませんでした。");
                    }
                    _player.targetTexture = _renderTexture;
                    _videoSurface.image = _renderTexture;
                }
                _videoSurface.style.display = DisplayStyle.None;
                _videoMatte.style.display = DisplayStyle.None;
                _blackout.style.opacity = 0f;
                _playbackError = null;
                _hasPlaybackEnded = false;
                _hasFirstFrame = false;
                _videoAudioGain = 0f;
                _canControlVideoAudio = false;
                _player.Prepare();
                float prepareStarted = Time.unscaledTime;
                while (!_player.isPrepared)
                {
                    ThrowIfPlaybackFailed();
                    if (Time.unscaledTime - prepareStarted > PREPARE_TIMEOUT_SECONDS)
                    {
                        throw new TimeoutException("PVの準備が制限時間内に完了しませんでした。");
                    }
                    await Awaitable.NextFrameAsync(token);
                }
                token.ThrowIfCancellationRequested();
                if (_player.audioTrackCount > 0)
                {
                    if (!_player.canSetDirectAudioVolume)
                    {
                        throw new InvalidOperationException("PV音声の音量制御に対応していません。");
                    }
                    _canControlVideoAudio = true;
                    _player.SetDirectAudioMute(VIDEO_AUDIO_TRACK, true);
                    ApplyVideoAudioVolume();
                }
                _layer.style.display = DisplayStyle.Flex;
                await FadeAsync(0f, 1f, 1f, 0f, token);
                _hasFirstFrame = false;
                _player.sendFrameReadyEvents = true;
                _player.Play();
                float firstFrameStarted = Time.unscaledTime;
                while (!_hasFirstFrame)
                {
                    ThrowIfPlaybackFailed();
                    if (Time.unscaledTime - firstFrameStarted > PREPARE_TIMEOUT_SECONDS)
                    {
                        throw new TimeoutException("PVの最初のフレームを確認できませんでした。");
                    }
                    await Awaitable.NextFrameAsync(token);
                }
                _videoMatte.style.display = DisplayStyle.Flex;
                _videoSurface.style.display = DisplayStyle.Flex;
                if (_canControlVideoAudio)
                {
                    ApplyVideoAudioVolume();
                    _player.SetDirectAudioMute(VIDEO_AUDIO_TRACK, false);
                }
                await FadeAsync(1f, 0f, 0f, 0f, token);
                float playbackStarted = Time.unscaledTime;
                while (!_hasPlaybackEnded)
                {
                    ThrowIfPlaybackFailed();
                    if (Time.unscaledTime - playbackStarted > (float)_player.length + PREPARE_TIMEOUT_SECONDS)
                    {
                        throw new TimeoutException("PVの再生終了を確認できませんでした。");
                    }
                    await Awaitable.NextFrameAsync(token);
                }
                await FadeAsync(0f, 1f, 0f, 0f, token);
                StopVideo();
                _videoSurface.style.display = DisplayStyle.None;
                _videoMatte.style.display = DisplayStyle.None;
                await FadeAsync(1f, 0f, 0f, 1f, token);
            }
            catch (OperationCanceledException)
            {
                // 入力や画面遷移による中断は正常な終了。
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[{nameof(TitleIdleVideoView)}] PV再生を中止します。{exception.Message}", this);
            }
            finally
            {
                if (generation == _generation)
                {
                    _cycleCancellation?.Dispose();
                    _cycleCancellation = null;
                    RestoreBackground();
                    _lastInputTime = Time.unscaledTime;
                }
            }
        }

        /// <summary>
        ///     黒背景とBGM倍率を2秒かけて変更し、PV音声は明転に合わせて再生します。
        /// </summary>
        private async Awaitable FadeAsync(float fromBlack, float toBlack, float fromGain, float toGain, CancellationToken token)
        {
            float started = Time.unscaledTime;
            while (true)
            {
                token.ThrowIfCancellationRequested();
                ThrowIfPlaybackFailed();
                float progress = Mathf.Clamp01((Time.unscaledTime - started) / FADE_SECONDS);
                float blackOpacity = Mathf.Lerp(fromBlack, toBlack, progress);
                _blackout.style.opacity = blackOpacity;
                _hasBgmOverride = true;
                _setBgmGain(Mathf.Lerp(fromGain, toGain, progress));
                _videoAudioGain = _hasFirstFrame && !_hasPlaybackEnded ? 1f - blackOpacity : 0f;
                ApplyVideoAudioVolume();
                if (progress >= 1f)
                {
                    return;
                }
                await Awaitable.NextFrameAsync(token);
            }
        }

        /// <summary>
        ///     旧世代の処理を先に無効化し、入力やシーン遷移を待たせず元の背景へ戻します。
        /// </summary>
        private void CancelCycle()
        {
            if (_cycleCancellation == null)
            {
                return;
            }
            _generation++;
            CancellationTokenSource cancellation = _cycleCancellation;
            _cycleCancellation = null;
            cancellation.Cancel();
            cancellation.Dispose();
            RestoreBackground();
        }

        /// <summary>
        ///     演出だけを解除し、最新の設定音量と既存タイトルUIを復元します。
        /// </summary>
        private void RestoreBackground()
        {
            StopVideo();
            if (_layer != null)
            {
                _layer.style.display = DisplayStyle.None;
            }
            if (_hasBgmOverride)
            {
                _setBgmGain?.Invoke(1f);
                _hasBgmOverride = false;
            }
        }

        /// <summary>
        ///     最新BGM設定にPV独自の明転倍率と終端2秒の減衰を掛けます。
        /// </summary>
        private void ApplyVideoAudioVolume()
        {
            if (!_canControlVideoAudio || _player == null || !_player.isPrepared)
            {
                return;
            }

            float endingGain = _player.length > 0d
                ? Mathf.Clamp01((float)(_player.length - _player.time) / FADE_SECONDS)
                : 1f;
            float settingsVolume = Mathf.Clamp01(_getBgmVolume());
            _player.SetDirectAudioVolume(VIDEO_AUDIO_TRACK, settingsVolume * _videoAudioGain * endingGain);
        }

        /// <summary>
        ///     PV音声をミュートしてから動画を停止し、次回の再生前状態へ戻します。
        /// </summary>
        private void StopVideo()
        {
            if (_player != null)
            {
                if (_canControlVideoAudio && _player.isPrepared)
                {
                    _player.SetDirectAudioMute(VIDEO_AUDIO_TRACK, true);
                }
                _player.Stop();
                _player.sendFrameReadyEvents = false;
            }
            _videoAudioGain = 0f;
            _canControlVideoAudio = false;
        }

        /// <summary>
        ///     非同期の動画エラーを演出中断へ変換します。
        /// </summary>
        private void ThrowIfPlaybackFailed()
        {
            if (!string.IsNullOrEmpty(_playbackError))
            {
                throw new InvalidOperationException(_playbackError);
            }
        }

        /// <summary>
        ///     キー、ポインター、タッチ、ゲームパッドの有効入力を調べます。
        /// </summary>
        private static bool HasInput()
        {
            if (Keyboard.current?.anyKey.isPressed == true || Touchscreen.current?.primaryTouch.press.isPressed == true)
            {
                return true;
            }
            Mouse mouse = Mouse.current;
            if (mouse != null && (mouse.delta.ReadValue().sqrMagnitude > 0f || mouse.scroll.ReadValue().sqrMagnitude > 0f
                || mouse.leftButton.isPressed || mouse.rightButton.isPressed || mouse.middleButton.isPressed))
            {
                return true;
            }
            float deadzone = InputSystem.settings.defaultDeadzoneMin;
            foreach (Gamepad gamepad in Gamepad.all)
            {
                if (gamepad.leftStick.ReadUnprocessedValue().sqrMagnitude > deadzone * deadzone
                    || gamepad.rightStick.ReadUnprocessedValue().sqrMagnitude > deadzone * deadzone)
                {
                    return true;
                }
                foreach (InputControl control in gamepad.allControls)
                {
                    if (control is ButtonControl button && button.isPressed)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        ///     背景演出要素を親いっぱいに配置します。
        /// </summary>
        private static void FillParent(VisualElement element)
        {
            element.style.position = Position.Absolute;
            element.style.left = 0;
            element.style.top = 0;
            element.style.right = 0;
            element.style.bottom = 0;
        }
    }
}
