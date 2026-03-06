using UnityEngine;
using DevelopProducts.Architecture.Adaptor;
using CharacterController = DevelopProducts.Architecture.Adaptor.CharacterController;

namespace DevelopProducts.Architecture.View
{
    public class CharacterView : MonoBehaviour
    {
        public void SetController(CharacterController controller)
        {
            _controller = controller;
        }

        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        private CharacterController _controller;
    }
}
