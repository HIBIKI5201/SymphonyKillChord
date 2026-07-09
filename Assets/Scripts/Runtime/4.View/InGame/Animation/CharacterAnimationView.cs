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
            _playableController.PlayOneShot(index);

            float clipLength = _playableController.GetClipLength(index);
            float speed = Mathf.Max(0.0001f, _locomotionCalculator.AnimationSpeed);
            _overlayDuration = clipLength / speed;
            _overlayRemaining = _overlayDuration;
            _overlayIndex = index;
            _shouldNotifyDodgeEnded = request.ShouldNotifyDodgeEnded;
            _hasNotifiedOneShotEnded = false;
        }

        /// <summary>
        ///     ワンショットのオーバーレイウェイトを反映する。
        /// </summary>
        private void ApplyOverlayWeight()
        {
            if (_overlayRemaining <= 0f || _weights == null || _overlayIndex < 0 || _overlayIndex >= _weights.Length)
            {
                return;
            }

            float elapsed = _overlayDuration - _overlayRemaining;
            float rampUp = Mathf.Max(0.001f, _overlayDuration * _attackRampUpRatio);
            float weight;

            if (elapsed < rampUp)
            {
                weight = Mathf.InverseLerp(0f, rampUp, elapsed);
            }
            else
            {
                float downElapsed = elapsed - rampUp;
                float downDuration = Mathf.Max(0.0001f, _overlayDuration - rampUp);
                weight = Mathf.Lerp(1f, 0f, downElapsed / downDuration);
            }

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

            _overlayRemaining -= Time.deltaTime;
            if (_overlayRemaining <= 0f && !_hasNotifiedOneShotEnded)
            {
                _hasNotifiedOneShotEnded = true;
                if (_shouldNotifyDodgeEnded
                    && _context.Signal is CharacterAnimationSignal signal)
                {
                    signal.NotifyDodgeEnded();
                }
            }
        }

        [SerializeField, Tooltip("ワンショット再生時の立ち上がり比率です。")]
        private float _attackRampUpRatio = 0.25f;

        [SerializeField, Tooltip("Playableを駆動するAnimatorです。")]
        private Animator _animator;

        private PlayableAnimationController _playableController;
        private CharacterAnimationLocomotionCalculator _locomotionCalculator;
        private ICharacterAnimationViewContext _context;
        private MusicSyncState _musicSyncState;
        private float[] _weights;
        private bool _isInitialized;
        private float _overlayDuration;
        private float _overlayRemaining;
        private int _overlayIndex = -1;
        private bool _shouldNotifyDodgeEnded;
        private bool _hasNotifiedOneShotEnded;
    }
}
