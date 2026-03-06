using UnityEngine;
using DevelopProducts.Architecture.Adaptor;
using CharacterController = DevelopProducts.Architecture.Adaptor.CharacterController;
using DevelopProducts.Architecture.Domain;

namespace DevelopProducts.Architecture.View
{
    [RequireComponent(typeof(Rigidbody))]
    public class CharacterView : MonoBehaviour, ICharacterView
    {
        public void SetController(CharacterController controller)
        {
            _controller = controller;
        }

        void ICharacterView.Move(Vector2 vel)
        {
            _rb.AddForce(new Vector3(vel.x, 0, vel.y), ForceMode.Impulse);
        }

        private Rigidbody _rb;
        private CharacterController _controller;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.TryGetComponent(out CharacterView view))
            {
                view.TakeDmage();
                _controller.AddDamage();
            }
        }
    }
}
