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
        ///     会心ダメージボーナスを適用して攻撃定義オブジェクトを生成する。
        ///     会心率は武器ではなくキャラクターが持つため、ここでは扱わない。
        /// </summary>
        /// <param name="data"> 攻撃定義データ。 </param>
        /// <param name="criticalMultiplierAddition"> 会心ダメージ倍率の加算値。 </param>
        /// <returns> 生成された攻撃定義オブジェクト。 </returns>
        public static AttackDefinition Create(
            AttackDefinitionAsset data,
            float criticalMultiplierAddition)
        {
            // 必要なアセットが揃っているかを確認する。
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

            // 会心倍率に加算分を足して攻撃仕様を作る。
            AttackSpec attackSpec = new AttackSpec(
                new CriticalMultiplier(
                    data.CriticalDamageMultiplier + criticalMultiplierAddition),
                new Damage(data.AttackSpecAsset.ConfirmedDamage)
            );

            // 拍の種類を使う場合は、有効な値かを確認する。
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

            // 各値を有効な範囲に収めて攻撃定義を作る。
            return new AttackDefinition(
                data.AttackName,
                attackSpec,
                data.AttackPipelineAsset.Create(),
                resolvedBeatType,
                data.JustDamageMultiplier,
                Mathf.Max(0f, data.WeaponDamageMultiplier),
                Mathf.Max(0f, data.Range),
                Mathf.Clamp(
                    data.HalfAngleDegrees,
                    AttackDefinition.MIN_HALF_ANGLE_DEGREES,
                    AttackDefinition.MAX_HALF_ANGLE_DEGREES),
                data.IsMultiTarget,
                Mathf.Max(AttackDefinition.MIN_HIT_COUNT, data.HitCount),
                Mathf.Max(0f, data.HitInterval),
                Mathf.Clamp(
                    data.OutOfRangeDamageMultiplier,
                    AttackDefinition.MIN_OUT_OF_RANGE_DAMAGE_MULTIPLIER,
                    AttackDefinition.MAX_OUT_OF_RANGE_DAMAGE_MULTIPLIER)
            );
        }
    }
}
