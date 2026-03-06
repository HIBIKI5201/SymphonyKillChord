using DevelopProducts.Architecture.Domain;
using DevelopProducts.Architecture.InfraStructure;
using DevelopProducts.Architecture.View;
using UnityEngine;
using CharacterController = DevelopProducts.Architecture.Adaptor.CharacterController;

namespace DevelopProducts.Architecture.Composition
{
    public class Initializer : MonoBehaviour
    {
        public void Awake()
        {
            CharacterEntity entity = new(_characterStatus.Name, _characterStatus.Health);
            CharacterController controller = new(entity);
            _characterView.SetController(controller);
            _buffer.SetController(controller);
        }

        [SerializeField]
        private CharacterStatus _characterStatus;
        [SerializeField]
        private CharacterView _characterView;
        [SerializeField]
        private InputBuffer _buffer;
    }
}
