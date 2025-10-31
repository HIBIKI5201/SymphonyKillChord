using UnityEngine;

namespace Mock.TPS
{
    public class PlayerManager : MonoBehaviour
    {
        private PlayerMover _playerMover;

        private void OnEnable()
        {
            _playerMover = new PlayerMover(transform, Camera.main.transform);
        }

        private void Update()
        {
            Vector3 inputDirection = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
            transform.position += _playerMover.CalcPlayerVelocityByInputDirection(in inputDirection);
        }
    }
}