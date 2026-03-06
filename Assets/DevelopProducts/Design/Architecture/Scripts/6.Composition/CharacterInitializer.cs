using DevelopProducts.Architecture.Adaptor;
using DevelopProducts.Architecture.Application;
using DevelopProducts.Architecture.Domain;
using DevelopProducts.Architecture.InfraStructure;
using DevelopProducts.Architecture.View;
using UnityEngine;
using CharacterController = DevelopProducts.Architecture.Adaptor.CharacterController;

namespace DevelopProducts.Architecture.Composition
{
    [RequireComponent(typeof(CharacterView))]
    public class CharacterInitializer : MonoBehaviour
    {
        public void Initialize()
        {
            CharacterView view = GetComponent<CharacterView>();

            CharacterEntity entity = new(_characterStatus.Name, _characterStatus.Health, _characterStatus.Speed);
            CharacterAttack characterAttack = new(entity);
            CharacterPresenter presenter = new(entity, view);
            CharacterController controller = new(characterAttack, presenter);
            view.SetController(controller);

            _controller = controller;
        }

        public void BindInputBuffer(InputBuffer buffer)
        {
            buffer.SetController(_controller);
        }

        [SerializeField]
        private CharacterStatus _characterStatus;

        private CharacterController _controller;
    }
}
