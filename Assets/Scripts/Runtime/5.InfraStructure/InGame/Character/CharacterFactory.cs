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
            return Create(data, 1f, 1f, 0f, 0f);
        }

        /// <summary>
        ///     CharacterDefinitionAssetへプレイヤーステータスボーナスを適用してCharacterEntityを生成する。
        /// </summary>
        /// <param name="data"> キャラクター定義データ。 </param>
        /// <param name="maxHealthMultiplier"> 最大体力の倍率。 </param>
        /// <param name="attackPowerMultiplier"> 攻撃力の倍率。 </param>
        /// <param name="criticalChanceAddition"> 会心率の加算値。 </param>
        /// <param name="criticalMultiplierAddition"> 会心ダメージ倍率の加算値。 </param>
        /// <returns> 生成したキャラクターEntity。 </returns>
        public static CharacterEntity Create(
            CharacterDefinitionAsset data,
            float maxHealthMultiplier,
            float attackPowerMultiplier,
            float criticalChanceAddition,
            float criticalMultiplierAddition)
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
                attackDefinitions[i] = AttackDefinitionFactory.Create(
                    attackDefinitionAssets[i],
                    criticalMultiplierAddition);
            }

            CharacterCombatSpec combatSpec = new CharacterCombatSpec(attackDefinitions);

            // 会心率はキャラクターが持つため、加算値もここで適用する。
            CriticalChance criticalChance = new CriticalChance(
                UnityEngine.Mathf.Clamp01(data.CriticalChance + criticalChanceAddition));

            return new CharacterEntity(
                new CharacterName(data.CharacterName),
                new HealthEntity(data.MaxHealth * maxHealthMultiplier),
                combatSpec,
                new AttackInterval(data.AttackInterval),
                new Damage(data.BaseDamage * attackPowerMultiplier),
                new BuffSystem(),
                criticalChance
            );
        }
    }
}

