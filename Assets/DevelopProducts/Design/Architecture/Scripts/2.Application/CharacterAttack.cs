using DevelopProducts.Architecture.Domain;
using UnityEngine;

namespace DevelopProducts.Architecture.Application
{
    public class CharacterAttack
    {
        public CharacterAttack(CharacterEntity entity, AttackPipeline pipeline)
        {
            _entity = entity;
            _pipeline = pipeline;
        }

        /// <summary>
        ///     対象にダメージを与える。
        /// </summary>
        /// <param name="target"> ダメージを受ける対象。 </param>
        public void AddDamage(CharacterEntity target)
        {

            DamageContext damage = new(_entity.AttackPower);
            damage = _pipeline.Process(damage);
            target.TakeDamage(damage);
        }

        private readonly CharacterEntity _entity;
        private readonly AttackPipeline _pipeline;
    }
}
