using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Utility;
using UnityEngine;

namespace KillChord.Runtime.View
{
    [DefaultExecutionOrder(ExecutionOrderConst.MOVEMENT)]
    public sealed class PlayerView : MonoBehaviour
    {
        [SerializeField] private string _blendName;
        [SerializeField] private Animator _animator;
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private Transform _cameraTransform;
        public void Init(PlayerController playerMovementController)
        {
            _controller = playerMovementController;
        }
        void Start()
        {
            Debug.Assert(_rb != null, $"{nameof(_rb)}がNull", this);
            Debug.Assert(_animator != null, $"{nameof(_animator)}がNull", this);
            Debug.Assert(_cameraTransform != null, $"{nameof(_cameraTransform)}がNull", this);
            Debug.Assert(_controller != null, $"{nameof(_controller)}がNullです。Update()更新前にInit()を実行するようにしてください。", this);

            _cacheTransform = transform;
        }
        void Update()
        {
            UpdateMovement();
        }
        private void UpdateMovement()
        {
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


        private Transform _cacheTransform;
        private PlayerController _controller;

    }
}
