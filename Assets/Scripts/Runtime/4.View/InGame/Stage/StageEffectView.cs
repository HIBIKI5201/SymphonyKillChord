using KillChord.Runtime.Adaptor.InGame.Stage;
using KillChord.Runtime.View.InGame.Character;
using SymphonyFrameWork.Attribute;
using System;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Stage
{
    /// <summary>
    ///     ステージ演出IDに対応するワンショット演出を再生します。
    /// </summary>
    public sealed class StageEffectView : MonoBehaviour
    {
        /// <summary>
        ///     ステージ演出ViewModelを設定します。
        /// </summary>
        /// <param name="viewModel"> 購読するViewModelです。 </param>
        public void Initialize(IStageEffectViewModel viewModel)
        {
            Shutdown();
            _viewModel = viewModel;
            if (_viewModel != null)
            {
                _viewModel.OnEffectRequested += HandleEffectRequested;
            }
        }

        /// <summary>
        ///     ViewModelの購読を解除します。
        /// </summary>
        public void Shutdown()
        {
            if (_viewModel != null)
            {
                _viewModel.OnEffectRequested -= HandleEffectRequested;
                _viewModel = null;
            }
        }

        [SerializeField, Tooltip("ステージ演出IDと再生内容の対応一覧です。")]
        private StageEffectBinding[] _bindings = Array.Empty<StageEffectBinding>();

        private IStageEffectViewModel _viewModel;

        /// <summary>
        ///     破棄時にイベント購読を解除します。
        /// </summary>
        private void OnDestroy()
        {
            Shutdown();
        }

        /// <summary>
        ///     対応するステージ演出を再生します。
        /// </summary>
        /// <param name="effectId"> 演出IDです。 </param>
        /// <param name="kind"> 演出種類です。 </param>
        private void HandleEffectRequested(
            string effectId,
            StageEffectViewKind kind)
        {
            StageEffectBinding[] bindings = _bindings ?? Array.Empty<StageEffectBinding>();
            for (int i = 0; i < bindings.Length; i++)
            {
                StageEffectBinding binding = bindings[i];
                if (binding != null && binding.Matches(effectId, kind))
                {
                    binding.Play(effectId);
                }
            }
        }

        /// <summary>
        ///     演出IDと再生内容の対応を保持します。
        /// </summary>
        [Serializable]
        private sealed class StageEffectBinding
        {
            /// <summary>
            ///     指定された演出要求と一致するか判定します。
            /// </summary>
            /// <param name="effectId"> 演出IDです。 </param>
            /// <param name="kind"> 演出種類です。 </param>
            /// <returns> 一致する場合はtrueです。 </returns>
            public bool Matches(string effectId, StageEffectViewKind kind)
            {
                return string.Equals(_effectId, effectId, StringComparison.Ordinal)
                    && _kind == kind;
            }

            /// <summary>
            ///     登録された演出を再生します。
            /// </summary>
            /// <param name="effectId"> CueNameとして渡す演出IDです。 </param>
            public void Play(string effectId)
            {
                IOneShotVisualEffect[] effects =
                    _effects ?? Array.Empty<IOneShotVisualEffect>();
                for (int i = 0; i < effects.Length; i++)
                {
                    effects[i]?.Play(effectId);
                }
            }

            [SerializeField, Tooltip("StageEffectAsset側と対応する演出IDです。")]
            private string _effectId;

            [SerializeField, Tooltip("このBindingが処理する演出種類です。")]
            private StageEffectViewKind _kind;

            [SerializeReference, SubclassSelector, Tooltip("演出要求時に再生するワンショット演出です。")]
            private IOneShotVisualEffect[] _effects = Array.Empty<IOneShotVisualEffect>();
        }
    }

    /// <summary>
    ///     GameObjectの有効状態を切り替えるワンショット演出です。
    /// </summary>
    [Serializable]
    public sealed class GameObjectActivationOneShotVisualEffect : IOneShotVisualEffect
    {
        /// <summary>
        ///     GameObjectを指定状態へ切り替えます。
        /// </summary>
        /// <param name="cueName"> 使用しません。 </param>
        public void Play(string cueName)
        {
            _target?.SetActive(_isActiveOnPlay);
        }

        /// <summary>
        ///     GameObjectを再生時と逆の状態へ戻します。
        /// </summary>
        public void Stop()
        {
            _target?.SetActive(!_isActiveOnPlay);
        }

        [SerializeField, Tooltip("有効状態を切り替えるGameObjectです。")]
        private GameObject _target;

        [SerializeField, Tooltip("再生時に設定する有効状態です。")]
        private bool _isActiveOnPlay = true;
    }

    /// <summary>
    ///     AnimatorのTriggerを発火するワンショット演出です。
    /// </summary>
    [Serializable]
    public sealed class AnimatorTriggerOneShotVisualEffect : IOneShotVisualEffect
    {
        /// <summary>
        ///     AnimatorのTriggerを発火します。
        /// </summary>
        /// <param name="cueName"> 使用しません。 </param>
        public void Play(string cueName)
        {
            if (_animator == null || string.IsNullOrWhiteSpace(_triggerName))
            {
                return;
            }

            _animator.ResetTrigger(_triggerName);
            _animator.SetTrigger(_triggerName);
        }

        /// <summary>
        ///     Triggerをリセットします。
        /// </summary>
        public void Stop()
        {
            if (_animator != null && !string.IsNullOrWhiteSpace(_triggerName))
            {
                _animator.ResetTrigger(_triggerName);
            }
        }

        [SerializeField, Tooltip("Triggerを発火するAnimatorです。")]
        private Animator _animator;

        [SerializeField, Tooltip("発火するTrigger名です。")]
        private string _triggerName;
    }
}
