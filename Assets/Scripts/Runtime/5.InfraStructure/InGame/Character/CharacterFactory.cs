using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.InfraStructure.InGame.Battle;
using System;
using System.Collections.Generic;

namespace KillChord.Runtime.InfraStructure.InGame.Character
{
    /// <summary>
    ///     CharacterDataからCharacterEntityを生成するクラス。
    /// </summary>
    public sealed class CharacterFactory
    {
        /// <summary>
        ///     CharacterDataからCharacterEntityを生成する。
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public CharacterEntity Create(CharacterData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            if (data.AttackDifinitions == null)
            {
                throw new ArgumentException("AttackDifinitions must not be null.", nameof(data));
            }

            Dictionary<AttackId, AttackDefinition> definitions = new Dictionary<AttackId, AttackDefinition>();

            AttackDefinitionData[] attackDefinitions = data.AttackDifinitions;
            for (int i = 0; i < attackDefinitions.Length; i++)
            {
                AttackDefinitionData attackDefinition = attackDefinitions[i];
                if (attackDefinition == null)
                {
                    throw new ArgumentException($"AttackDifinitions[{i}] must not be null.", nameof(data));
                }
                definitions.Add(attackDefinition.AttackId, new AttackDefinition(
                    attackDefinition.AttackId,
                    new Damage(attackDefinition.BaseDamage)));
            }

            CharacterCombatSpec combatSpec = new CharacterCombatSpec(definitions);

            return new CharacterEntity(
                data.CharacterName,
                new HealthEntity(data.MaxHealth),
                new MoveSpeed(data.MoveSpeed),
                new AttackPower(data.AttackPower),
                combatSpec);
        }
    }
}