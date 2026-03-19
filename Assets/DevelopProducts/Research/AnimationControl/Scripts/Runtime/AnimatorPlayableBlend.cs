using DevelopProducts.AnimationControl.Adaptor;
using System;
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

        public void Play(AnimationClip clip)
        {
            if (_clipPlayable.IsValid())
            {
                _graph.Disconnect(_mixer, 1);
                _clipPlayable.Destroy();
            }

            _clipPlayable = AnimationClipPlayable.Create(_graph, clip);

            _graph.Connect(_clipPlayable, 0, _mixer, 1);

            _clipPlayable.SetTime(0);
            _clipPlayable.SetDone(false);

            _playingClip = true;

            _mixer.SetInputWeight(0, 0f);
            _mixer.SetInputWeight(1, 1f);

            _currentClip = clip;
        }

        public void Update()
        {
            if (!_playingClip) { return; }

            if (_clipPlayable.GetTime() >= _currentClip.length)
            {
                _playingClip = false;

                _mixer.SetInputWeight(0, 1f);
                _mixer.SetInputWeight(1, 0f);
            }
        }

        public void Dispose()
        {
            if (_graph.IsValid())
            {
                _graph.Destroy();
            }
        }

        private const string ANIMATION_OUTPUT_NAME = "AnimationOutput";
        private const string GRAPH_NAME = "AnimatorPlayableBlend";

        private readonly Animator _animator;
        private readonly RuntimeAnimatorController _controller;

        private PlayableGraph _graph;
        private AnimationMixerPlayable _mixer;
        private AnimationClipPlayable _clipPlayable;
        private AnimatorControllerPlayable _controllerPlayable;

        private AnimationClip _currentClip;
        private bool _playingClip;

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

            // 空Clipで初期化
            _clipPlayable =
                AnimationClipPlayable.Create(_graph, new AnimationClip());

            _graph.Connect(_controllerPlayable, 0, _mixer, 0);
            _graph.Connect(_clipPlayable, 0, _mixer, 1);

            _mixer.SetInputWeight(0, 1f);
            _mixer.SetInputWeight(1, 0f);

            output.SetSourcePlayable(_mixer);

            _graph.Play();
        }
    }
}
