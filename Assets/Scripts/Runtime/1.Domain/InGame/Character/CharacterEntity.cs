using KillChord.Runtime.Domain.InGame.Battle;
using System;

namespace KillChord.Runtime.Domain.InGame.Character
{
    /// <summary>
    ///     キャラクターの基本的な情報を保持するクラス。
    /// </summary>
    public class CharacterEntity : IAttacker, IDefender
    {
        /// <summary>
        ///     コンストラクタ。
        /// </summary>
        /// <param name="name"></param>
        /// <param name="health"></param>
        /// <param name="moveSpeed"></param>
        /// <param name="attackPower"></param>
        /// <param name="combatSpec"></param>
        public CharacterEntity(CharacterName name,
            HealthEntity health,
            MoveSpeed moveSpeed,
            AttackPower attackPower,
            CharacterCombatSpec combatSpec
            )
        {
            if (health is null)
                throw new ArgumentNullException(nameof(health));
            if (combatSpec is null)
                throw new ArgumentNullException(nameof(combatSpec));

            Name = name;
            Health = health;
            MoveSpeed = moveSpeed;
            AttackPower = attackPower;
            CombatSpec = combatSpec;
        }

        public CharacterName Name { get; }
        public HealthEntity Health { get; }
        public MoveSpeed MoveSpeed { get; }
        public AttackPower AttackPower { get; }
        public CharacterCombatSpec CombatSpec { get; }
        public Health CurrentHelth => Health.CurrentHealth;
        public Health MaxHealth => Health.MaxHealth;

        /// <summary>
        ///     ダメージを受ける処理。
        ///     HealthEntityのChangeHealthを呼び出す。
        /// </summary>
        /// <param name="damage"></param>
        public void TakeDamage(Damage damage)
        {
            Health nextHealth = new Health(CurrentHelth.Value - damage.Value);
            Health.ChangeHealth(nextHealth);
        }

        /// <summary>
        ///     HPを回復する処理。
        /// </summary>
        /// <param name="healAmount"></param>
        public void Heal(Health healAmount)
        {
            Health nextHealth = new Health(CurrentHelth.Value + healAmount.Value);
            Health.ChangeHealth(nextHealth);
        }
    }
}
