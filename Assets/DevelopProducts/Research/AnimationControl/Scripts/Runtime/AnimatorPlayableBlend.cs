using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace DevelopProducts.AnimationControl.Blender
{
    public class AnimatorPlayableBlend : MonoBehaviour
    {
        public Animator animator;
        public RuntimeAnimatorController controller;
        public AnimationClip specialClip;
        public KeyCode triggerKey = KeyCode.Space;

        PlayableGraph graph;
        AnimationMixerPlayable mixer;
        AnimationClipPlayable clipPlayable;
        AnimatorControllerPlayable controllerPlayable;

        bool playingClip = false;

        void Start()
        {
            graph = PlayableGraph.Create("PlayableGraph");

            var output = AnimationPlayableOutput.Create(
                graph,
                "AnimationOutput",
                animator
            );

            mixer = AnimationMixerPlayable.Create(graph, 2);

            controllerPlayable =
                AnimatorControllerPlayable.Create(graph, controller);

            clipPlayable =
                AnimationClipPlayable.Create(graph, specialClip);

            graph.Connect(controllerPlayable, 0, mixer, 0);
            graph.Connect(clipPlayable, 0, mixer, 1);

            mixer.SetInputWeight(0, 1f);
            mixer.SetInputWeight(1, 0f);

            output.SetSourcePlayable(mixer);

            graph.Play();
        }

        void Update()
        {
            // スペースキーでアニメ再生
            if (Input.GetKeyDown(triggerKey) && !playingClip)
            {
                Debug.Log("Play Special Clip");
                clipPlayable.SetTime(0);
                playingClip = true;

                mixer.SetInputWeight(0, 0f);
                mixer.SetInputWeight(1, 1f);
            }

            // 再生終了チェック
            if (playingClip)
            {
                if (clipPlayable.GetTime() >= specialClip.length)
                {
                    playingClip = false;

                    mixer.SetInputWeight(0, 1f);
                    mixer.SetInputWeight(1, 0f);
                }
            }
        }

        void OnDestroy()
        {
            graph.Destroy();
        }
    }
}
