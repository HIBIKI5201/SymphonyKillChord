using System;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.Music;

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
                new CriticalChance(data.AttackSpecAsset.CriticalChance),
                new CriticalMultiplier(data.AttackSpecAsset.CriticalDamageMultiplier),
                new Damage(data.AttackSpecAsset.ConfirmedDamage)
            );

            int? beatType = data.UseBeatType ? data.BeatType : null;

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
