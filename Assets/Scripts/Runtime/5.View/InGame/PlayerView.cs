using KillChord.Runtime.Adaptor;
using UnityEngine;

namespace KillChord.Runtime.View
{
    public sealed class PlayerView : MonoBehaviour
    {
        [SerializeField] private string _blendName;
        [SerializeField] private Animator _animator;
        [SerializeField] private Transform _cameraTransform;
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

            dir = Rotate(dir, -_cameraTransform.eulerAngles.y);

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                _controller.TryDodge(dir, Time.time);
            }

            Vector3 position = _cacheTransform.position;
            Quaternion rotation = _cacheTransform.rotation;
            _controller.Update(ref position, ref rotation, dir, Time.time, Time.deltaTime);
            _cacheTransform.SetPositionAndRotation(position, rotation);
        }

        private static Vector2 Rotate(Vector2 v, float degrees)
        {
            float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
            float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

            return new Vector2(cos * v.x - sin * v.y, sin * v.x + cos * v.y);
        }

        private Transform _cacheTransform;
        private PlayerController _controller;

    }
}
