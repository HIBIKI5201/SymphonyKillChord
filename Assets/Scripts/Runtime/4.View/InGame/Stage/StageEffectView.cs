using KillChord.Runtime.Adaptor.InGame.Stage;
using KillChord.Runtime.Utility.Identity;
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
            int effectId,
            StageEffectViewKind kind)
        {
            StageEffectBinding[] bindings = _bindings ?? Array.Empty<StageEffectBinding>();
            for (int i = 0; i < bindings.Length; i++)
            {
                StageEffectBinding binding = bindings[i];
                if (binding != null && binding.Matches(effectId, kind))
                {
                    binding.Play();
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
            public bool Matches(int effectId, StageEffectViewKind kind)
            {
                return _effectId.Id == effectId
                    && _kind == kind;
            }

            /// <summary>
            ///     登録された演出を再生します。
            /// </summary>
            public void Play()
            {
                IOneShotVisualEffect[] effects =
                    _effects ?? Array.Empty<IOneShotVisualEffect>();
                for (int i = 0; i < effects.Length; i++)
                {
                    effects[i]?.Play(_cueName);
                }
            }

            [SerializeField, DataCategory("StageEffect"), Tooltip("StageEffectAsset側と対応する演出IDです。")]
            private DataID _effectId;

            [SerializeField, Tooltip("このBindingが処理する演出種類です。")]
            private StageEffectViewKind _kind;

            [SerializeField, Tooltip("音声演出へ渡すCRI Cue名です。音声を使用しない場合は空にします。")]
            private string _cueName;

            [SerializeReference, SubclassSelector, Tooltip("演出要求時に再生するワンショット演出です。")]
            private IOneShotVisualEffect[] _effects = Array.Empty<IOneShotVisualEffect>();
        }
    }
}
