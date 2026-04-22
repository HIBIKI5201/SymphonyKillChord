using DevelopProducts.AnimationControl.Adaptor;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace DevelopProducts.AnimationControl.Blender
{
    /// <summary>
    ///     AnimatorControllerとAnimationClipをPlayableでブレンドするクラス。
    /// </summary>
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
        public void Register(AnimationBlendClipRequest request)
        {
            AnimationClip clip = request.Clip;
            if (clip == null) { return; }

            if (_clipMap.ContainsKey(clip))
            {
                return;
            }
            // AnimationClip → Playable化。
            AnimationClipPlayable clipPlayable = AnimationClipPlayable.Create(_graph, clip);
            // ルートモーションやIKの影響を受けないように設定。
            clipPlayable.SetApplyFootIK(false);
            clipPlayable.SetApplyPlayableIK(false);

            var blendClip = new AnimationBlendClipData(
                request,
                clipPlayable
            );

            _clipMap.Add(clip, blendClip);
        }

        /// <summary>
        ///     再生（Enter/Exit時間指定）。
        /// </summary>
        public void Play(AnimationClip clip)
        {
            if (clip == null) { return; }

            if (!_clipMap.TryGetValue(clip, out AnimationBlendClipData blendClip))
            {
                Register(new(clip));
                blendClip = _clipMap[clip];
            }

            if (_currentBlendClip.IsValid)
            {
                _graph.Disconnect(_mixer, 1);
            }

            var playable = blendClip.ClipPlayable;
            // PlayableGraphに接続。
            _graph.Connect(playable, 0, _mixer, 1);
            // 再生開始位置をリセット。
            playable.SetTime(0);
            // 「再生終了扱い」を解除（再利用対策）。
            playable.SetDone(false);

            _currentClip = clip;
            _currentBlendClip = blendClip;

            _enterDuration = Mathf.Max(blendClip.EnterDuration, 0.0001f);
            _exitDuration = Mathf.Max(blendClip.ExitDuration, 0.0001f);
            // ブレンド時間リセット。
            _blendTime = 0f;
            // ブレンド開始。
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
                            // ブレンド完了 → Clip再生状態へ。
                            _state = BlendState.Play;
                        }
                        break;
                    }

                case BlendState.Play:
                    {
                        // Clipの再生時間を監視して、Exit開始タイミングを判断。
                        double time = _currentBlendClip.ClipPlayable.GetTime();
                        // Clipの長さからExit開始までの時間を引いた値が、現在の再生時間を超えたらExit開始。
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
                            // ブレンド完了 → Clip停止＆状態リセット。
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