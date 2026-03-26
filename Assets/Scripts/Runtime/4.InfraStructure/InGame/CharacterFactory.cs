using System.Collections.Generic;
using KillChord.Runtime.Domain;

namespace KillChord.Runtime.InfraStructure
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
            Dictionary<AttackId, AttackDefinition> definitions = new Dictionary<AttackId, AttackDefinition>();

            AttackDefinitionData[] attackDefinitions = data.AttackDifinitions;
            for (int i = 0; i < attackDefinitions.Length; i++)
            {
                AttackDefinitionData attackDefinition = attackDefinitions[i];
                definitions[attackDefinition.AttackId] = new AttackDefinition(
                    attackDefinition.AttackId,
                    new Damage(attackDefinition.BaseDamage));
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