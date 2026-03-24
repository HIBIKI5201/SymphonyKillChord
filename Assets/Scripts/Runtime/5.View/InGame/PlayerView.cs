using KillChord.Runtime.Adaptor;
using UnityEngine;

namespace KillChord.Runtime.View
{
    public sealed class PlayerView : MonoBehaviour
    {
        [SerializeField] private string _blendName;
        [SerializeField] private Animator _animator;

        public void Init(PlayerController playerMovementController)
        {
            _controller = playerMovementController;
        }
        void Start()
        {
            _cacheTransform = transform;
        }
        void Update()
        {
            UpdateAnimation();
            UpdateMovement();
            UpdateDodge();
        }
        private void UpdateAnimation()
        {
            Vector2 dir = Vector2.zero;
            dir.x = Input.GetAxis("Horizontal");
            dir.y = Input.GetAxis("Vertical");

            _animator.SetFloat(_blendName, Mathf.Min(1f, dir.magnitude));
        }
        private void UpdateMovement()
        {
            Vector2 dir = Vector2.zero;
            dir.x = Input.GetAxis("Horizontal");
            dir.y = Input.GetAxis("Vertical");
            if (dir == Vector2.zero)
                return;

            _cacheTransform.position = _controller.GetMovedPosition(_cacheTransform.position, dir, Time.deltaTime);
            _cacheTransform.rotation = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.y), Vector3.up);
        }
        private void UpdateDodge()
        {
            if (!Input.GetKeyDown(KeyCode.LeftShift))
                return;

            Vector2 dir = Vector2.zero;
            dir.x = Input.GetAxis("Horizontal");
            dir.y = Input.GetAxis("Vertical");
            if (dir == Vector2.zero)
                return;

            _cacheTransform.position = _controller.GetDodgedPosition(_cacheTransform.position, dir, Time.time);
            _cacheTransform.rotation = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.y), Vector3.up);
        }

        private Transform _cacheTransform;
        private PlayerController _controller;
    }
}
