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

        // =========================================
        // Clip登録（事前生成）
        // =========================================
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

            var blendClip = new AnimationBlendClip(
                clip,
                clipPlayable
            );

            _clipMap.Add(clip, blendClip);
        }

        // =========================================
        // 再生
        // =========================================
        public void Play(AnimationClip clip)
        {
            if (clip == null) { return; }

            // 未登録なら自動登録
            if (!_clipMap.TryGetValue(clip, out AnimationBlendClip blendClip))
            {
                Register(clip);
                blendClip = _clipMap[clip];
            }

            // 既存接続を解除
            if (_currentBlendClip.IsValid)
            {
                _graph.Disconnect(_mixer, 1);
            }

            var playable = blendClip.ClipPlayable;

            _graph.Connect(playable, 0, _mixer, 1);

            playable.SetTime(0);
            playable.SetDone(false);

            _mixer.SetInputWeight(0, 0f);
            _mixer.SetInputWeight(1, 1f);

            _playingClip = true;

            _currentClip = clip;
            _currentBlendClip = blendClip;
        }

        // =========================================
        // 更新
        // =========================================
        public void Update()
        {
            if (!_playingClip) { return; }

            if (!_currentBlendClip.IsValid) { return; }

            if (_currentBlendClip.ClipPlayable.GetTime() >= _currentClip.length)
            {
                _playingClip = false;

                // Controllerへ戻す
                _mixer.SetInputWeight(0, 1f);
                _mixer.SetInputWeight(1, 0f);
            }
        }

        // =========================================
        // 解放
        // =========================================
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

        private readonly Dictionary<AnimationClip, AnimationBlendClip> _clipMap = new();

        private AnimationBlendClip _currentBlendClip;
        private AnimationClip _currentClip;

        private bool _playingClip;

        // =========================================
        // 初期化
        // =========================================
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

            // Controllerを0番に接続（常時待機）
            _graph.Connect(_controllerPlayable, 0, _mixer, 0);

            _mixer.SetInputWeight(0, 1f); // Controller
            _mixer.SetInputWeight(1, 0f); // Clip

            output.SetSourcePlayable(_mixer);

            _graph.Play();
        }

        // =========================================
        // 内部データ構造
        // =========================================
        private readonly struct AnimationBlendClip
        {
            public readonly AnimationClip Clip;
            public readonly AnimationClipPlayable ClipPlayable;

            public bool IsValid => ClipPlayable.IsValid();

            public AnimationBlendClip(
                AnimationClip clip,
                AnimationClipPlayable clipPlayable
            )
            {
                Clip = clip;
                ClipPlayable = clipPlayable;
            }
        }
    }
}