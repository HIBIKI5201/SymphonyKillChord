using KillChord.Runtime.Adaptor.InGame.Battle;
using KillChord.Runtime.Adaptor.InGame.Player;
using KillChord.Runtime.Adaptor.InGame.Skill;
using KillChord.Runtime.Utility;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Player
{
    /// <summary>
    ///     プレイヤーの見た目や制御を管理するViewクラス。
    /// </summary>
    [DefaultExecutionOrder(ExecutionOrderConst.MOVEMENT)]
    public sealed class PlayerView : MonoBehaviour, IDamageable
    {
        [SerializeField] private string _blendName;
        [SerializeField] private Animator _animator;
        [SerializeField] private Rigidbody _rb;

        private Transform _cameraTransform;
        private bool _isInitialized = false;

        public BattleController BattleController => _battleController;

        public void Init(
            PlayerController playerMovementController,
            BattleController battleController,
            Transform cameraTransform)
        {
            _controller = playerMovementController;
            _battleController = battleController;
            _cameraTransform = cameraTransform;
            _colliders = new Collider[8];
            Debug.Assert(_rb != null, $"{nameof(_rb)}がNull", this);
            Debug.Assert(_animator != null, $"{nameof(_animator)}がNull", this);
            Debug.Assert(_cameraTransform != null, $"{nameof(_cameraTransform)}がNull", this);
            _cacheTransform = transform;
            _isInitialized = true;
        }

        void Update()
        {
            if (!_isInitialized || _controller == null) return;
            UpdateMovement();

            if (_battleController != null && Input.GetKeyDown(KeyCode.Mouse0))
            {
                int length = Physics.OverlapSphereNonAlloc(_cacheTransform.position, 3f, _colliders);
                for (int i = 0; i < length; i++)
                {
                    if (!_colliders[i].TryGetComponent(out IDamageable damageable))
                        continue;
                    if (this is IDamageable myDamageable && myDamageable == damageable)
                        continue;
                    // _battleController.Attack(damageable.BattleController);
                    Debug.Log($"{gameObject.name}から{_colliders[i].name}へ攻撃", this);
                }
            }
        }

        private void UpdateMovement()
        {
            if (_controller == null) return;
            Vector2 dir = Vector2.zero;
            dir.x = Input.GetAxis("Horizontal");
            dir.y = Input.GetAxis("Vertical");

            _animator.SetFloat(_blendName, Mathf.Min(1f, dir.magnitude));

            dir = Rotate(dir, -_cameraTransform.eulerAngles.y);

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                _controller.TryDodge(dir, Time.time);
            }

            Quaternion rotation = _cacheTransform.rotation;
            _controller.Update(ref rotation, dir, Time.time, out Vector3 velocity);
            _rb.linearVelocity = velocity;

            _cacheTransform.rotation = rotation;
        }

        private static Vector2 Rotate(Vector2 v, float degrees)
            => Quaternion.Euler(0, 0, degrees) * v;


        private Collider[] _colliders;
        private Transform _cacheTransform;
        private PlayerController _controller;
        private BattleController _battleController;
    }
}