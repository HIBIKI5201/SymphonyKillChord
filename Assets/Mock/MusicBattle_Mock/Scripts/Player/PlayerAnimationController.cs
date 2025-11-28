using UnityEngine;

namespace Mock.MusicBattle
{
    public class PlayerAnimationController : MonoBehaviour
    {
        /// <summary>
        ///   速度をアニメーターに設定する。
        /// </summary>
        /// <param name="value"></param>
        public void MoveVelocity(float value)
        {
            _animator?.SetFloat(_moveVelocityHash, value);
        }

        private Animator _animator;
        [SerializeField] private string _moveVelocity = "MoveVelocity";
        private int _moveVelocityHash;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void OnValidate()
        {
            _moveVelocityHash = Animator.StringToHash(_moveVelocity);
        }
    }
}
