using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.Music;
using System;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Battle
{
    /// <summary>
    ///     攻撃定義をScriptableObjectから生成するファクトリークラス。
    /// </summary>
    public static class AttackDefinitionFactory
    {
        /// <summary>
        ///     攻撃定義データを受け取り、攻撃定義オブジェクトを生成するメソッド。
        /// </summary>
        /// <param name="data"> 攻撃定義データ。 </param>
        /// <returns> 生成された攻撃定義オブジェクト。 </returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static AttackDefinition Create(AttackDefinitionAsset data)
        {
            return Create(data, 0f, 0f);
        }

        /// <summary>
        ///     会心ボーナスを適用して攻撃定義オブジェクトを生成する。
        /// </summary>
        /// <param name="data"> 攻撃定義データ。 </param>
        /// <param name="criticalChanceAddition"> 会心率の加算値。 </param>
        /// <param name="criticalMultiplierAddition"> 会心ダメージ倍率の加算値。 </param>
        /// <returns> 生成された攻撃定義オブジェクト。 </returns>
        public static AttackDefinition Create(
            AttackDefinitionAsset data,
            float criticalChanceAddition,
            float criticalMultiplierAddition)
        {
            if (data == null)
            {
                throw new System.ArgumentNullException(nameof(data));
            }

            if (data.AttackSpecAsset == null)
            {
                throw new System.ArgumentNullException(nameof(data.AttackSpecAsset));
            }

            if (data.AttackPipelineAsset == null)
            {
                throw new System.ArgumentNullException(nameof(data.AttackPipelineAsset));
            }

            AttackSpec attackSpec = new AttackSpec(
                new CriticalChance(Mathf.Clamp01(
                    data.AttackSpecAsset.CriticalChance + criticalChanceAddition)),
                new CriticalMultiplier(
                    data.AttackSpecAsset.CriticalDamageMultiplier + criticalMultiplierAddition),
                new Damage(data.AttackSpecAsset.ConfirmedDamage)
            );

            int? beatType = data.UseBeatType ? (int?)data.BeatType : null;

            BeatType? resolvedBeatType = null;
            if (beatType.HasValue)
            {
                if (!Enum.IsDefined(typeof(BeatType), beatType.Value))
                {
                    throw new ArgumentException(
                        $"BeatType の値 {beatType.Value} は無効です。アセット '{data.AttackName}' を確認してください。",
                        nameof(data));
                }
                resolvedBeatType = (BeatType)beatType.Value;
            }

            return new AttackDefinition(
                data.AttackName,
                attackSpec,
                data.AttackPipelineAsset.Create(),
                resolvedBeatType,
                data.JustDamageMultiplier
            );
        }
    }
}
