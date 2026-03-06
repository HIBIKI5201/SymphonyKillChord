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

        /// <summary>
        ///     対象にダメージを与える。
        /// </summary>
        /// <param name="target"> ダメージを受ける対象。 </param>
        public void AddDamage(CharacterEntity target)
        {
            target.TakeDamage(_entity.AttackPower);
        }

        private readonly CharacterEntity _entity;
    }
}
