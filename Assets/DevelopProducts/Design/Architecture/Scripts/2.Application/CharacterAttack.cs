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

        private readonly CharacterEntity _entity;
    }
}
