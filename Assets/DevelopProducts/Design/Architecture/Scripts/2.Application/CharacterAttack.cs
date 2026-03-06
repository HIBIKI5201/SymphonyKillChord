using DevelopProducts.Architecture.Domain;
using UnityEngine;

namespace DevelopProducts.Architecture.Application
{
    public class CharacterAttack
    {
        public CharacterAttack(CharacterEntity entity)
        {
            _entity = entity;
        }

        public void AddDamage(CharacterEntity target)
        {

        }

        private readonly CharacterEntity _entity;
    }
}
