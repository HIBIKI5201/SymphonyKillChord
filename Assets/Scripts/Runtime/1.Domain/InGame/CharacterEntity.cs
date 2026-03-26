using UnityEngine;

namespace KillChord.Runtime.Domain
{
    /// <summary>
    ///     キャラクターの基本的な情報を保持するクラス。
    /// </summary>
    public class CharacterEntity : IHitTarget
    {
        /// <summary>
        ///     コンストラクタ。
        /// </summary>
        /// <param name="name"></param>
        /// <param name="health"></param>
        /// <param name="moveSpeed"></param>
        /// <param name="attackPower"></param>
        /// <param name="combatSpec"></param>
        public CharacterEntity(string name,
            HealthEntity health, 
            MoveSpeed moveSpeed,
            AttackPower attackPower, 
            CharacterCombatSpec combatSpec
            )
        {
            Name = name;
            Health = health;
            MoveSpeed = moveSpeed;
            AttackPower = attackPower;
            CombatSpec = combatSpec;
        }

        public string Name { get; }
        public HealthEntity Health { get; }
        public MoveSpeed MoveSpeed { get; }
        public AttackPower AttackPower { get; }
        public CharacterCombatSpec CombatSpec { get; }
    }
}
