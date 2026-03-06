using DevelopProducts.Architecture.Domain;
using UnityEngine;

namespace DevelopProducts.Architecture.Application
{
    /// <summary>
    ///     キャラクターの攻撃処理を担当するアプリケーションサービス。
    /// </summary>
    public class CharacterAttack
    {
        /// <summary>
        ///     コンストラクタ。
        /// </summary>
        /// <param name="entity"> 攻撃を行うエンティティ。 </param>
        /// <param name="pipeline"> 使用する攻撃パイプライン。 </param>
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
