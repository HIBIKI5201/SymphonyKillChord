using DevelopProducts.AnimationControl.Adaptor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DevelopProducts.AnimationControl.Blender
{
    /// <summary>
    ///     アニメーションの操作を管理する。
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class AnimationManager : MonoBehaviour
    {
        [SerializeField]
        private Key _forwardKey = Key.W;
        [SerializeField]
        private Key _backKey = Key.S;
        [SerializeField]
        private Key _rightKey = Key.D;
        [SerializeField]
        private Key _leftKey = Key.A;

        [SerializeField, Range(0, 1)]
        private float _acceleration = 0.95f;

        [SerializeField]
        private AnimationClip _playClip;
        [SerializeField]
        private Key _playKey = Key.Space;

        private Animator _animator;
        private RuntimeAnimatorController _controller;
        private AnimatorPlayableBlend _blender;
        private SymphonyAnimeAdaptor _adaptor;

        private Vector2 _velocity;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _controller = _animator.runtimeAnimatorController;
            _adaptor = new SymphonyAnimeAdaptor(_animator);
            _blender = new AnimatorPlayableBlend(_adaptor);
        }

        private void Update()
        {
            Vector2 acc = Vector2.zero;

            if (Keyboard.current[_forwardKey].isPressed)
            {
                acc += Vector2.up;
            }
            if (Keyboard.current[_backKey].isPressed)
            {
                acc += Vector2.down;
            }
            if (Keyboard.current[_rightKey].isPressed)
            {
                acc += Vector2.right;
            }
            if (Keyboard.current[_leftKey].isPressed)
            {
                acc += Vector2.left;
            }


            if (Keyboard.current[_playKey].wasPressedThisFrame)
            {
                _blender.Play(_playClip);
            }

            _velocity = Vector2.Lerp(_velocity, acc, _acceleration * Time.deltaTime);

            _adaptor.SetVelocity(_velocity);
            _blender?.Update();
        }

        private void OnDestroy()
        {
            _blender?.Dispose();
        }
    }
}
