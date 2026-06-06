
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.Music;

namespace KillChord.Runtime.Domain.Player
{
    /// <summary>
    ///     スキル効果の発動に必要な情報をまとめた構造体。
    /// </summary>
    public readonly struct SkillEffectContext
    {
        public SkillEffectContext( CharacterEntity targetEntity, CharacterEntity playerEntity, BeatType currentBeatType)
        {
            TargetEntity = targetEntity;
            PlayerEntity = playerEntity;
            CurrentBeatType = currentBeatType;
        }
        /// <summary>
        /// 被スキル者
        /// </summary>
        public CharacterEntity TargetEntity { get; }
        /// <summary>
        /// スキル発動者
        /// </summary>
        public CharacterEntity PlayerEntity { get; }
        
        /// <summary>
        /// 現在のビートタイプ。スキル効果の計算や条件判定に使用される。
        /// </summary>
        public BeatType CurrentBeatType { get; }


    }
}