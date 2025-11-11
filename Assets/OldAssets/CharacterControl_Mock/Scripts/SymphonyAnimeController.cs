using System;
using UnityEngine;

namespace Mock.CharacterControl
{
    /// <summary>
    ///     Symphonyのアニメーションを管理するクラス
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    public class SymphonyAnimeController : MonoBehaviour, IRootMotionReciever
    {
        public event Action<bool> OnRootMotionChanged;
        public event Action OnAnimatorMoveAction;

        public bool IsRootMotion => _animator.applyRootMotion;

        void IRootMotionReciever.ActiveRootMotion()
        {
            _animator.applyRootMotion = true;
            OnRootMotionChanged?.Invoke(true);
        }
        void IRootMotionReciever.InactiveRootMotion()
        {
            _animator.applyRootMotion = false;
            OnRootMotionChanged?.Invoke(false);
        }
        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void OnAnimatorMove()
        {
            OnAnimatorMoveAction?.Invoke();
        }
    }
}
