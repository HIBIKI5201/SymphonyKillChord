using KillChord.Runtime.Application.InGame.Buff;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.InfraStructure.InGame.Battle;
using System;

namespace KillChord.Runtime.InfraStructure.InGame.Character
{
    /// <summary>
    ///     CharacterDefinitionAssetからCharacterEntityを生成するクラス。
    /// </summary>
    public static class CharacterFactory
    {
        /// <summary>
        ///     CharacterDefinitionAssetからCharacterEntityを生成する。
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static CharacterEntity Create(CharacterDefinitionAsset data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            AttackDefinitionAsset[] attackDefinitionAssets = data.AttackDefinitionAssets;
            if (attackDefinitionAssets == null)
            {
                throw new ArgumentException("AttackDefinitionAssets must not be null.", nameof(data));
            }

            AttackDefinition[] attackDefinitions = new AttackDefinition[attackDefinitionAssets.Length];
            for (int i = 0; i < attackDefinitions.Length; i++)
            {
                attackDefinitions[i] = AttackDefinitionFactory.Create(attackDefinitionAssets[i]);
            }

            CharacterCombatSpec combatSpec = new CharacterCombatSpec(attackDefinitions);

            return new CharacterEntity(
                new CharacterName(data.CharacterName),
                new HealthEntity(data.MaxHealth),
                combatSpec,
                new AttackInterval(data.AttackInterval),
                new Damage(data.BaseDamage),
                new BuffSystem()
            );
        }
    }
}

