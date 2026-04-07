using DevelopProducts.AnimationControl.Adaptor;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace DevelopProducts.AnimationControl.Blender
{
    public class AnimatorPlayableBlend : IDisposable
    {
        public AnimatorPlayableBlend(AnimationAdaptor adaptor)
        {
            _animator = adaptor.Animator;
            _controller = adaptor.Controller;

            Initialize();
        }

        /// <summary>
        ///     Clip登録（事前生成）。
        /// </summary>
        public void Register(AnimationClip clip)
        {
            if (clip == null) { return; }

            if (_clipMap.ContainsKey(clip))
            {
                return;
            }

            var clipPlayable = AnimationClipPlayable.Create(_graph, clip);

            clipPlayable.SetApplyFootIK(false);
            clipPlayable.SetApplyPlayableIK(false);

            var blendClip = new AnimationBlendClipData(
                clip,
                clipPlayable
            );

            _clipMap.Add(clip, blendClip);
        }

        /// <summary>
        ///     再生（Enter/Exit時間指定）。
        /// </summary>
        public void Play(AnimationClip clip, float enterDuration = 0.1f, float exitDuration = 0.1f)
        {
            if (clip == null) { return; }

            if (!_clipMap.TryGetValue(clip, out AnimationBlendClipData blendClip))
            {
                Register(clip);
                blendClip = _clipMap[clip];
            }

            if (_currentBlendClip.IsValid)
            {
                _graph.Disconnect(_mixer, 1);
            }

            var playable = blendClip.ClipPlayable;

            _graph.Connect(playable, 0, _mixer, 1);

            playable.SetTime(0);
            playable.SetDone(false);

            _currentClip = clip;
            _currentBlendClip = blendClip;

            _enterDuration = Mathf.Max(enterDuration, 0.0001f);
            _exitDuration = Mathf.Max(exitDuration, 0.0001f);

            _blendTime = 0f;
            _state = BlendState.Enter;
        }

        /// <summary>
        ///     更新。
        /// </summary>
        public void Update()
        {
            if (!_currentBlendClip.IsValid) { return; }

            float deltaTime = Time.deltaTime;
            
            switch (_state)
            {
                case BlendState.Enter:
                {
                    _blendTime += deltaTime;

                    float t = Mathf.Clamp01(_blendTime / _enterDuration);

                    // Controller → Clip
                    _mixer.SetInputWeight(0, 1f - t);
                    _mixer.SetInputWeight(1, t);

                    if (t >= 1f)
                    {
                        _state = BlendState.Play;
                    }
                    break;
                }

                case BlendState.Play:
                {
                    double time = _currentBlendClip.ClipPlayable.GetTime();
                    float exitStartTime = Mathf.Max(0f, _currentClip.length - _exitDuration);

                    if (time >= exitStartTime)
                    {
                        _blendTime = 0f;
                        _state = BlendState.Exit;
                    }
                    break;
                }

                case BlendState.Exit:
                {
                    _blendTime += deltaTime;

                    float t = Mathf.Clamp01(_blendTime / _exitDuration);

                    // Clip → Controller
                    _mixer.SetInputWeight(0, t);
                    _mixer.SetInputWeight(1, 1f - t);

                    if (t >= 1f)
                    {
                        _state = BlendState.None;
                    }
                    break;
                }
            }
        }

        /// <summary>
        ///     解放。
        /// </summary>
        public void Dispose()
        {
            if (_graph.IsValid())
            {
                _graph.Destroy();
            }

            _clipMap.Clear();
        }

        private const string ANIMATION_OUTPUT_NAME = "AnimationOutput";
        private const string GRAPH_NAME = "AnimatorPlayableBlend";

        private readonly Animator _animator;
        private readonly RuntimeAnimatorController _controller;

        private PlayableGraph _graph;
        private AnimationMixerPlayable _mixer;
        private AnimatorControllerPlayable _controllerPlayable;

        private readonly Dictionary<AnimationClip, AnimationBlendClipData> _clipMap = new();

        private AnimationBlendClipData _currentBlendClip;
        private AnimationClip _currentClip;

        private float _enterDuration;
        private float _exitDuration;
        private float _blendTime;

        private BlendState _state;

        /// <summary>
        ///     初期化。
        /// </summary>
        private void Initialize()
        {
            _graph = PlayableGraph.Create(GRAPH_NAME);

            var output = AnimationPlayableOutput.Create(
                _graph,
                ANIMATION_OUTPUT_NAME,
                _animator
            );

            _mixer = AnimationMixerPlayable.Create(_graph, 2);

            _controllerPlayable =
                AnimatorControllerPlayable.Create(_graph, _controller);

            _graph.Connect(_controllerPlayable, 0, _mixer, 0);

            _mixer.SetInputWeight(0, 1f);
            _mixer.SetInputWeight(1, 0f);

            output.SetSourcePlayable(_mixer);

            _graph.Play();
        }

        /// <summary>
        ///     ブレンド状態。
        /// </summary>
        private enum BlendState
        {
            None,
            Enter,
            Play,
            Exit
        }
    }
}