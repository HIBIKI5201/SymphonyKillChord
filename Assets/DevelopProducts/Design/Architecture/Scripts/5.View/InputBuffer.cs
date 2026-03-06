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

        private CharacterController _controller;
    }
}
