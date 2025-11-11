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
        public bool IsRootMotion 
        {
            get => _animator.applyRootMotion;
            set => _animator.applyRootMotion = value;
        }

        void IRootMotionReciever.ActiveRootMotion() => _animator.applyRootMotion = true;
        void IRootMotionReciever.InactiveRootMotion() => _animator.applyRootMotion = false;

        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }
    }
}
