
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
        /// スキルの効果が対象とするキャラクターエンティティ。スキル効果の実行に必要な情報を提供する。
        /// </summary>
        public CharacterEntity TargetEntity { get; }
        /// <summary>
        /// プレイヤーの基礎攻撃力。スキル効果の計算に使用される。
        /// </summary>
        public CharacterEntity PlayerEntity { get; }
        
        /// <summary>
        /// 現在のビートタイプ。スキル効果の計算や条件判定に使用される。
        /// </summary>
        public BeatType CurrentBeatType { get; }


    }
}