using KillChord.Runtime.Adaptor.InGame.Animation;
using KillChord.Runtime.Adaptor.InGame.Music;
using System;
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
                signal.OnCancelRequested += HandleCancelRequestedHandler;
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

            float target = _context.ViewModel.IsReserving ? 1f : 0f;
            _reserveBlend = Mathf.MoveTowards(
                _reserveBlend, target, Time.deltaTime / Mathf.Max(0.0001f, _reserveBlendSeconds));

            if (_reserveBlend > 0f)
            {
                for (int i = 0; i < _weights.Length; i++)
                {
                    _weights[i] *= (1f - _reserveBlend);   // ロコモーションを退ける
                }
                int reservedIndex = (int)CharacterAnimationClipType.Reserved;
                _weights[reservedIndex] = Mathf.Max(_weights[reservedIndex], _reserveBlend);
            }

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
                signal.OnCancelRequested -= HandleCancelRequestedHandler;
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
            _playableController.PlayOneShot(index);
            _overlayIndex = index;
            _overlayBaseDuration = request.BaseDurationSeconds;
            _overlayEnterBlendDuration = request.EnterBlendDurationSeconds;
            _overlayExitBlendDuration = request.ExitBlendDurationSeconds;
            _overlayElapsedBaseTime = 0f;
            _shouldNotifyDodgeEnded = request.ShouldNotifyDodgeEnded;
            _hasNotifiedOneShotEnded = false;
        }

        /// <summary>
        ///     再生中のワンショットを途中終了させ、ロコモーションへ戻す。
        /// </summary>
        private void HandleCancelRequestedHandler()
        {
            if (!HasActiveOverlay())
            {
                return;
            }

            // 経過時間をexitブレンド開始位置まで進め、既存の終了補間でロコモーションへ戻す。
            float exitBlendDuration = Mathf.Clamp(_overlayExitBlendDuration, 0f, _overlayBaseDuration);

            if (exitBlendDuration <= 0f)
            {
                CompleteOverlay();
                return;
            }

            float exitBlendStart = Mathf.Clamp(_overlayBaseDuration - exitBlendDuration, 0f, _overlayBaseDuration);
            _overlayElapsedBaseTime = Mathf.Max(_overlayElapsedBaseTime, exitBlendStart);
        }

        /// <summary>
        ///     ワンショットの終了処理を行い、オーバーレイ状態を解除する。
        /// </summary>
        private void CompleteOverlay()
        {
            _overlayElapsedBaseTime = _overlayBaseDuration;

            if (_hasNotifiedOneShotEnded)
            {
                return;
            }

            _hasNotifiedOneShotEnded = true;
            if (_shouldNotifyDodgeEnded && _context.Signal is CharacterAnimationSignal signal)
            {
                signal.NotifyDodgeEnded();
            }

            _overlayIndex = -1;
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

            float weight = CalculateOverlayWeight();

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

            _overlayElapsedBaseTime += _oneShotTimingCalculator.GetBaseProgressDelta(
                Time.deltaTime,
                _locomotionCalculator.AnimationSpeed);

            if (_overlayElapsedBaseTime >= _overlayBaseDuration && !_hasNotifiedOneShotEnded)
            {
                _hasNotifiedOneShotEnded = true;
                if (_shouldNotifyDodgeEnded
                    && _context.Signal is CharacterAnimationSignal signal)
                {
                    signal.NotifyDodgeEnded();
                }

                _overlayIndex = -1;
            }
        }

        [SerializeField, Tooltip("Playableを駆動するAnimatorです。")]
        private Animator _animator;
        [SerializeField, Tooltip("予約ブレンド時間です。")]
        private float _reserveBlendSeconds = 0.1f;

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
        private float _reserveBlend;
        private int _overlayIndex = -1;
        private bool _shouldNotifyDodgeEnded;
        private bool _hasNotifiedOneShotEnded;

        /// <summary>
        ///     ワンショットオーバーレイが有効か判定する。
        /// </summary>
        /// <returns> 有効な場合はtrue。 </returns>
        private bool HasActiveOverlay()
        {
            return _overlayBaseDuration > 0f
                && _overlayElapsedBaseTime < _overlayBaseDuration
                && _weights != null
                && _overlayIndex >= 0
                && _overlayIndex < _weights.Length;
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
