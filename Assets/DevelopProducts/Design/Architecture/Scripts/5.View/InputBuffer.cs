using UnityEngine;
using CharacterController = DevelopProducts.Architecture.Adaptor.CharacterController;

namespace DevelopProducts.Architecture.View
{
    public class InputBuffer : MonoBehaviour
    {
        public void SetController(CharacterController controller)
        {
            _controller = controller;
        }

        public void Update()
        {
            bool pressedW = Input.GetKey(KeyCode.W);
            bool pressedA = Input.GetKey(KeyCode.A);
            bool pressedS = Input.GetKey(KeyCode.S);
            bool pressedD = Input.GetKey(KeyCode.D);

            Vector2 dir = Vector2.zero;
            if (pressedW)
            {
                dir += Vector2.up;
            }
            if (pressedA)
            {
                dir += Vector2.left;
            }
            if (pressedS)
            {
                dir += Vector2.down;
            }
            if (pressedD)
            {
                dir += Vector2.right;
            }

            if (pressedW || pressedA || pressedS || pressedD)
            {
                _controller.Move(dir);
            }
        }

        private CharacterController _controller;
    }
}
