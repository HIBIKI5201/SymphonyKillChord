using DevelopProducts.Architecture.Application;
using DevelopProducts.Architecture.Domain;
using UnityEngine;

namespace DevelopProducts.Architecture.Adaptor
{
    public class CharacterController
    {
        public CharacterController(CharacterAttack attacker, CharacterPresenter presenter)
        {
            _attacker = attacker;
            _presenter = presenter;
        }

        public void AddDamage(CharacterPresenter target)
        {
            _attacker.AddDamage(target);
        }

        public void Move(Vector2 dir)
        {
            Debug.Log($"Move: {dir}");
            _presenter.Move(dir);
        }

        private CharacterPresenter _presenter;
        private CharacterAttack _attacker;
    }
}
