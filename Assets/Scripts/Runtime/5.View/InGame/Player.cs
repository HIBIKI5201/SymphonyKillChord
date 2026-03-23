using KillChord.Runtime.Adaptor;
using UnityEngine;

namespace KillChord.Runtime.View
{
    public sealed class Player : MonoBehaviour
    {
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
            UpdateMovement();
            UpdateDash();
        }


        private void UpdateMovement()
        {
            Vector2 dir = Vector2.zero;
            dir.x = Input.GetAxis("Horizontal");
            dir.y = Input.GetAxis("Vertical");
            if (dir == Vector2.zero)
                return;

            _cacheTransform.position = _controller.GetMovedPosition(_cacheTransform.position, dir, Time.deltaTime);
        }
        private void UpdateDash()
        {
            if (!Input.GetKeyDown(KeyCode.LeftShift))
                return;

            Vector2 dir = Vector2.zero;
            dir.x = Input.GetAxis("Horizontal");
            dir.y = Input.GetAxis("Vertical");
            if (dir == Vector2.zero)
                return;

            _cacheTransform.position = _controller.GetDashedPosition(_cacheTransform.position, dir, Time.time);
        }

        private Transform _cacheTransform;
        private PlayerController _controller;
    }
}
