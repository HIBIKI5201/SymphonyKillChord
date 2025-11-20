using UnityEngine;

namespace Mock.MusicBattle
{
    public class EnemyLockOnController
    {
        public EnemyLockOnController(Transform target)
        {
            _targetposition = target;
        }

        /// <summary> プレイヤーを設定する。</summary>
        public void SetTarget(Transform target)
        {
            _targetposition = target;
        }

        /// <summary>　ロックオン対象にする。</summary>
        public void SetLockOn()
        {
            _isLockedOn = true;
        }

        /// <summary>
        /// 射程内までプレイヤーに近づく。
        /// </summary>
        private void MoveTo(Transform _enemyposition, Transform _targetposition)
        {
            if (_enemyposition == null && _targetposition == null) return;

            float distance = Vector3.Distance(_targetposition.position, _enemyposition.position);
            //射程外　＝＞　近づく。
            if (distance > _attackRange)
            {
                Vector3 direction = (_targetposition.position - _enemyposition.position).normalized;
                _rigidbody.linearVelocity = direction * _moveSpeed;
            }
            //射程内＝＞止まって攻撃処理へ。
            else
            {
                _rigidbody.linearVelocity = Vector3.zero;
            }
        }

        private float _moveSpeed;
        private float _attackRange;
        private Rigidbody _rigidbody;
        private bool _isLockedOn = false;
        private bool _isInRange = false;
        private Transform _targetposition;
        private Transform _enemyposition;
    }
}