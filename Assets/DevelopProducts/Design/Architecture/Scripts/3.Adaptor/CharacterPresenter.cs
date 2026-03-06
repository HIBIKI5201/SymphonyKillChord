using DevelopProducts.Architecture.Domain;
using UnityEngine;

namespace DevelopProducts.Architecture.Adaptor
{
    public class CharacterPresenter
    {
        public CharacterPresenter(CharacterEntity entity, ICharacterView view)
        {
            _entity = entity;
            _view = view;
        }

        public void Move(Vector2 dir)
        {
            Vector2 velocity = dir * _entity.Speed;
            Debug.Log($"Velocity {velocity}");
            _view.Move(dir);
        }

        private CharacterEntity _entity;
        private ICharacterView _view;
    }
}
