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
        /// <summary> ルートモーションのアクティブ状態が変化した時 </summary>
        public event Action<bool> OnRootMotionChanged;
        /// <summary> ルートモーションで移動した時 </summary> 
        public event Action OnAnimatorMoveAction;

        public bool IsRootMotion => _animator.applyRootMotion;
        public Vector3 DeltaPosition => _animator.deltaPosition;
        public Quaternion DeltaRotation => _animator.deltaRotation;

        public void AttackTrigger() => _animator?.SetTrigger(_attackTriggerHash);
        public void MoveSpeed(float value) => _animator?.SetFloat(_moveVelocityHash, value);

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

        [SerializeField, Delayed]
        private string _attackTriggerName = "Attack";
        [SerializeField, Delayed]
        private string _moveVelocityName = "MoveVelocity";

        private int _attackTriggerHash;
        private int _moveVelocityHash;
        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _animator.applyRootMotion = false;
        }

        private void OnAnimatorMove()
        {
            OnAnimatorMoveAction?.Invoke();
        }

        private void OnValidate()
        {
            _attackTriggerHash = Animator.StringToHash(_attackTriggerName);
            _moveVelocityHash = Animator.StringToHash(_moveVelocityName);
        }
    }
}
