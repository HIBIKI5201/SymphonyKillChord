using DevelopProducts.Architecture.Application;
using DevelopProducts.Architecture.Domain;
using UnityEngine;

namespace DevelopProducts.Architecture.Adaptor
{
    public class CharacterController
    {
        public CharacterController(CharacterEntity entity)
        {
            _attacker = new CharacterAttack(entity);
        }

        private CharacterAttack _attacker;
    }
}
