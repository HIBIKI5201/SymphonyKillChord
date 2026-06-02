
using KillChord.Runtime.Domain.InGame.Character;

namespace KillChord.Runtime.Domain.Player
{
    /// <summary>
    ///     スキル効果の発動に必要な情報をまとめた構造体。
    /// </summary>
    public readonly struct SkillEffectContext
    {
        public SkillEffectContext( CharacterEntity targetEntity)
        {
            TargetEntity = targetEntity;
        }

        public CharacterEntity TargetEntity { get; }


    }
}