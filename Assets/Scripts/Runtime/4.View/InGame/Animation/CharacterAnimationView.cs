using System;
using KillChord.Runtime.Adaptor.InGame.Animation;
using KillChord.Runtime.Adaptor.InGame.Music;
using UnityEngine;

namespace KillChord.Runtime.View
{
    /// <summary>
    ///     キャラクターアニメーションの再生と計算をView層で完結させる。
    /// </summary>
    public sealed class CharacterAnimationView : MonoBehaviour
    {
        /// <summary>
        ///     依存コンポーネントを注入してPlayableAnimationControllerを構築する。
        /// </summary>
        /// <param name="context"> アニメーションのView側依存。 </param>
        /// <param name="clips"> 再生対象のアニメーションクリップ一覧。 </param>
        /// <param name="musicSyncState"> BPM参照元。 </param>
        public void Initialize(
            CharacterAnimationViewContext context,
            AnimationClip[] clips,
            MusicSyncState musicSyncState)
        {
            _context = context;
            _musicSyncState = musicSyncState;
            _locomotionCalculator = new CharacterAnimationLocomotionCalculator();
            _oneShotTimingCalculator = new CharacterAnimationOneShotTimingCalculator();
            _weights = clips != null
                ? new float[clips.Length]
                : System.Array.Empty<float>();

            if (_animator == null)
            {
                _animator = GetComponent<Animator>();
            }

            _playableController = new PlayableAnimationController(_animator, clips);
            if (_context?.Signal is CharacterAnimationSignal signal)
            {
                signal.OnRequested += HandleRequestedHandler;
            }
            _isInitialized = true;
        }

        /// <summary>
        ///     毎フレーム、計算結果をPlayableへ反映する。
        /// </summary>
        private void Update()
        {
            if (!_isInitialized)
            {
                return;
            }

            if (_context == null)
            {
                return;
            }

            _locomotionCalculator.SetBpm(_musicSyncState != null ? (float)_musicSyncState.Bpm : 60f);
            _locomotionCalculator.SetVelocity(_context.ViewModel.Velocity);
            Array.Clear(_weights, 0, _weights.Length);
            _locomotionCalculator.ApplyBaseWeights(_weights);
            ApplyOverlayWeight();

            _playableController.SetAnimationSpeed(_locomotionCalculator.AnimationSpeed);
            for (int i = 0; i < _weights.Length; i++)
            {
                _playableController.SetWeight(i, _weights[i]);
            }
        }

        /// <summary>
        ///     PlayableGraphを破棄する。
        /// </summary>
        private void OnDestroy()
        {
            if (_context?.Signal is CharacterAnimationSignal signal)
            {
                signal.OnRequested -= HandleRequestedHandler;
            }

            _playableController?.Dispose();
        }

        /// <summary>
        ///     ワンショット再生を開始し、オーバーレイ状態を初期化する。
        /// </summary>
        /// <param name="request"> 再生要求です。 </param>
        private void HandleRequestedHandler(CharacterAnimationRequest request)
        {
            if (_playableController == null)
            {
                return;
            }

            int index = request.Index;
            bool shouldSkipEnterBlend = HasActiveOverlay() && _overlayIndex == index;
            _playableController.PlayOneShot(index);
            _overlayIndex = index;
            _overlayBaseDuration = request.BaseDurationSeconds;
            _overlayEnterBlendDuration = shouldSkipEnterBlend
                ? 0f
                : request.EnterBlendDurationSeconds;
            _overlayExitBlendDuration = request.ExitBlendDurationSeconds;
            _overlayElapsedBaseTime = 0f;
            _shouldNotifyDodgeEnded = request.ShouldNotifyDodgeEnded;
            _canCancelOverlayByMovement = request.CanCancelByMovement;
            _hasNotifiedOneShotEnded = false;
            ResetOverlayCancellation();
        }

        /// <summary>
        ///     ワンショットのオーバーレイウェイトを反映する。
        /// </summary>
        private void ApplyOverlayWeight()
        {
            if (!HasActiveOverlay())
            {
                return;
            }

            TryStartOverlayCancellation();
            float weight = _isOverlayCancelling
                ? CalculateOverlayCancellationWeight()
                : CalculateOverlayWeight();

            _weights[_overlayIndex] = Mathf.Max(_weights[_overlayIndex], weight);

            float otherScale = 1f - weight;
            for (int i = 0; i < _weights.Length; i++)
            {
                if (i == _overlayIndex)
                {
                    continue;
                }

                _weights[i] *= otherScale;
            }

            float progressDelta = _oneShotTimingCalculator.GetBaseProgressDelta(
                Time.deltaTime,
                _locomotionCalculator.AnimationSpeed);

            if (_isOverlayCancelling)
            {
                _overlayCancelElapsedBaseTime += progressDelta;
                if (_overlayCancelElapsedBaseTime >= _overlayExitBlendDuration)
                {
                    CompleteOverlay();
                }

                return;
            }

            _overlayElapsedBaseTime += progressDelta;
            if (_overlayElapsedBaseTime >= _overlayBaseDuration)
            {
                CompleteOverlay();
            }
        }

        [SerializeField, Tooltip("Playableを駆動するAnimatorです。")]
        private Animator _animator;

        private PlayableAnimationController _playableController;
        private CharacterAnimationLocomotionCalculator _locomotionCalculator;
        private CharacterAnimationOneShotTimingCalculator _oneShotTimingCalculator;
        private ICharacterAnimationViewContext _context;
        private MusicSyncState _musicSyncState;
        private float[] _weights;
        private bool _isInitialized;
        private float _overlayBaseDuration;
        private float _overlayEnterBlendDuration;
        private float _overlayExitBlendDuration;
        private float _overlayElapsedBaseTime;
        private float _overlayCancelStartWeight;
        private float _overlayCancelElapsedBaseTime;
        private int _overlayIndex = -1;
        private bool _shouldNotifyDodgeEnded;
        private bool _canCancelOverlayByMovement;
        private bool _isOverlayCancelling;
        private bool _hasNotifiedOneShotEnded;

        /// <summary>
        ///     ワンショットオーバーレイが有効か判定する。
        /// </summary>
        /// <returns> 有効な場合はtrue。 </returns>
        private bool HasActiveOverlay()
        {
            return _overlayBaseDuration > 0f
                && _weights != null
                && _overlayIndex >= 0
                && _overlayIndex < _weights.Length
                && (_isOverlayCancelling || _overlayElapsedBaseTime < _overlayBaseDuration);
        }

        /// <summary>
        ///     移動速度がしきい値を超えた場合にオーバーレイのキャンセルを開始する。
        /// </summary>
        private void TryStartOverlayCancellation()
        {
            if (!_canCancelOverlayByMovement || _isOverlayCancelling)
            {
                return;
            }

            float exitBlendDuration = Mathf.Clamp(_overlayExitBlendDuration, 0f, _overlayBaseDuration);
            float exitBlendStart = Mathf.Max(0f, _overlayBaseDuration - exitBlendDuration);
            if (_overlayElapsedBaseTime >= exitBlendStart)
            {
                return;
            }

            if (_context.ViewModel.Velocity.magnitude < CharacterAnimationLocomotionCalculator.WALK_THRESHOLD)
            {
                return;
            }

            _overlayCancelStartWeight = CalculateOverlayWeight();
            _overlayCancelElapsedBaseTime = 0f;
            _isOverlayCancelling = true;
        }

        /// <summary>
        ///     キャンセル開始時のウェイトから終了ブレンド時間をかけて0へ補間する。
        /// </summary>
        /// <returns> キャンセル中のオーバーレイウェイトです。 </returns>
        private float CalculateOverlayCancellationWeight()
        {
            if (_overlayExitBlendDuration <= 0f)
            {
                return 0f;
            }

            float progress = _overlayCancelElapsedBaseTime / _overlayExitBlendDuration;
            return Mathf.Lerp(_overlayCancelStartWeight, 0f, progress);
        }

        /// <summary>
        ///     オーバーレイを終了し、必要に応じて回避終了を通知する。
        /// </summary>
        private void CompleteOverlay()
        {
            if (!_hasNotifiedOneShotEnded)
            {
                _hasNotifiedOneShotEnded = true;
                if (_shouldNotifyDodgeEnded
                    && _context.Signal is CharacterAnimationSignal signal)
                {
                    signal.NotifyDodgeEnded();
                }
            }

            _overlayIndex = -1;
            _canCancelOverlayByMovement = false;
            ResetOverlayCancellation();
        }

        /// <summary>
        ///     移動キャンセルの進行状態を初期化する。
        /// </summary>
        private void ResetOverlayCancellation()
        {
            _isOverlayCancelling = false;
            _overlayCancelStartWeight = 0f;
            _overlayCancelElapsedBaseTime = 0f;
        }

        /// <summary>
        ///     現在のオーバーレイウェイトを計算する。
        /// </summary>
        /// <returns> オーバーレイウェイトです。 </returns>
        private float CalculateOverlayWeight()
        {
            if (_overlayBaseDuration <= 0f)
            {
                return 1f;
            }

            float enterBlendDuration = Mathf.Clamp(_overlayEnterBlendDuration, 0f, _overlayBaseDuration);
            float exitBlendDuration = Mathf.Clamp(_overlayExitBlendDuration, 0f, _overlayBaseDuration);
            float exitBlendStart = Mathf.Max(0f, _overlayBaseDuration - exitBlendDuration);

            if (enterBlendDuration > 0f && _overlayElapsedBaseTime < enterBlendDuration)
            {
                return Mathf.InverseLerp(0f, enterBlendDuration, _overlayElapsedBaseTime);
            }

            if (exitBlendDuration > 0f && _overlayElapsedBaseTime >= exitBlendStart)
            {
                return 1f - Mathf.InverseLerp(exitBlendStart, _overlayBaseDuration, _overlayElapsedBaseTime);
            }

            return 1f;
        }
    }
}
