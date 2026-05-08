using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Character;
using System;

namespace KillChord.Runtime.InfraStructure.InGame.Character
{
    /// <summary>
    ///     CharacterDataからCharacterEntityを生成するクラス。
    /// </summary>
    public static class CharacterFactory
    {
        /// <summary>
        ///     CharacterDataからCharacterEntityを生成する。
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static CharacterEntity Create(CharacterData data)
        {
            AttackDefinitionData[] attackDefinitionDatas = data.AttackDifinitions;
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            if (data.AttackDifinitions == null)
            {
                throw new ArgumentException("AttackDifinitions must not be null.", nameof(data));
            }

            AttackDefinition[] attackDefinitions = new AttackDefinition[attackDefinitionDatas.Length];
            for (int i = 0; i < attackDefinitions.Length; i++)
            {
                attackDefinitions[i] = AttackDefinitionFactory.Create(attackDefinitionDatas[i]);
            }

            CharacterCombatSpec combatSpec = new CharacterCombatSpec(attackDefinitions);

            return new CharacterEntity(
                new CharacterName(data.CharacterName),
                new HealthEntity(data.MaxHealth),
                combatSpec);
        }
    }
}