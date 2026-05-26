using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace KillChord.Runtime.View
{
    /// <summary>
    ///     PlayableGraphを構築してアニメーションクリップをブレンド再生する純粋クラス。
    ///     enumを基にDictionaryでStateとPlayableを管理するため、状態追加時もコード変更不要。
    /// </summary>
    public sealed class PlayableAnimationController : IDisposable
    {
        /// <summary> PlayableAnimationControllerを初期化してPlayableGraphを構築する。 </summary>
        /// <param name="animator"> アニメーションを再生するAnimator。 </param>
        /// <param name="clips"> アニメーションクリップの配列。 </param>
        public PlayableAnimationController(Animator animator, AnimationClip[] clips)
        {
            if (animator == null) throw new ArgumentNullException(nameof(animator));
            if (clips == null) throw new ArgumentNullException(nameof(clips));

            _clips = clips;
            _playables = new List<AnimationClipPlayable>(clips.Length);

            // PlayableGraphを構築する
            _graph = PlayableGraph.Create(GRAPH_NAME);
            _graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

            var output = AnimationPlayableOutput.Create(_graph, OUTPUT_NAME, animator);
            _mixer = AnimationMixerPlayable.Create(_graph, clips.Length);

            // インデックス順にclipをMixerへ登録する
            for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i] == null)
                {
                    throw new ArgumentException($"Clip at index {i} is null.", nameof(clips));
                }
                var playable = AnimationClipPlayable.Create(_graph, clips[i]);
                playable.SetApplyFootIK(false);
                playable.SetApplyPlayableIK(false);

                _graph.Connect(playable, 0, _mixer, i);
                _mixer.SetInputWeight(i, 0f);
                _playables.Add(playable);
            }

            // 初期状態をIndex0（Idle）に設定する
            _mixer.SetInputWeight(0, 1f);

            output.SetSourcePlayable(_mixer);
            _graph.Play();
        }

        /// <summary> 指定したStateのブレンドウェイトを設定する。 </summary>
        /// <param name="state"> アニメーション状態。 </param>
        /// <param name="weight"> ウェイト（0〜1）。 </param>
        public void SetWeight(int index, float weight)
        {
            if (index < 0 || index >= _playables.Count)
            {
                return;
            }

            _mixer.SetInputWeight(index, Mathf.Clamp01(weight));
        }

        /// <summary>
        ///     全Playableの再生速度を設定する。
        ///     bpm / 60f を渡すことでBPMに同期した速度で再生される。
        /// </summary>
        /// <param name="speed"> 再生速度（bpm / 60f）。 </param>
        public void SetAnimationSpeed(float speed)
        {
            foreach (var playable in _playables)
            {
                playable.SetSpeed(speed);
            }
        }

        // クリップを先頭に戻して再生位置をリセットする（先頭から確実に再生させたい時に呼ぶ）
        public void ReplayClipAtIndex(int index)
        {
            if (index < 0 || index >= _playables.Count) return;
            _playables[index].SetTime(0.0);
            // MixerでのウェイトはView側でフェードする想定なのでここでは再生位置のみリセットする
        }

        // 生のクリップ長（秒）を返す
        public float GetClipLength(int index)
        {
            if (index < 0 || index >= _clips.Length) return 0f;
            return _clips[index] != null ? _clips[index].length : 0f;
        }

        /// <summary> PlayableGraphを破棄する。 </summary>
        public void Dispose()
        {
            if (_graph.IsValid())
            {
                _graph.Destroy();
            }

            _playables.Clear();
        }
        private const string GRAPH_NAME = "CharacterAnimationGraph";
        private const string OUTPUT_NAME = "AnimationOutput";

        private readonly PlayableGraph _graph;
        private readonly AnimationMixerPlayable _mixer;
        // Playableをリストで管理することで、状態追加時もコード変更不要にする。
        private readonly List<AnimationClipPlayable> _playables;
        private readonly AnimationClip[] _clips;
    }
}
