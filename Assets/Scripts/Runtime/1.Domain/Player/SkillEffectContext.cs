
using KillChord.Runtime.Domain.InGame.Character;

namespace KillChord.Runtime.Domain.Player
{
    /// <summary>
    ///     スキル効果の発動に必要な情報をまとめた構造体。
    /// </summary>
    public readonly struct SkillEffectContext
    {
        public SkillEffectContext( CharacterEntity targetEntity, float playerBaseAttackPower)
        {
            TargetEntity = targetEntity;
            PlayerBaseAttackPower = playerBaseAttackPower;
        }
        /// <summary>
        /// スキルの効果が対象とするキャラクターエンティティ。スキル効果の実行に必要な情報を提供する。
        /// </summary>
        public CharacterEntity TargetEntity { get; }
        /// <summary>
        /// プレイヤーの基礎攻撃力。スキル効果の計算に使用される。
        /// </summary>
        public float PlayerBaseAttackPower { get; } 


    }
}