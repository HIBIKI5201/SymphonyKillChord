using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.Music;
using System;

namespace KillChord.Runtime.Domain.Player
{
    /// <summary>
    ///     スキル効果の発動に必要な情報をまとめた構造体。
    /// </summary>
    public readonly struct SkillEffectContext
    {
        /// <summary>
        ///     コンテキストを初期化します。
        /// </summary>
        /// <param name="targetEntity"> 主対象です。 </param>
        /// <param name="playerEntity"> 発動者です。 </param>
        /// <param name="currentBeatType"> 現在ビートです。 </param>
        /// <param name="targetEntities"> 解決済み対象一覧です。 </param>
        public SkillEffectContext(
            CharacterEntity targetEntity,
            CharacterEntity playerEntity,
            BeatType currentBeatType,
            ReadOnlyMemory<CharacterEntity> targetEntities)
        {
            TargetEntity = targetEntity;
            PlayerEntity = playerEntity;
            CurrentBeatType = currentBeatType;
            TargetEntities = targetEntities;
        }

        /// <summary> 被スキル者です。 </summary>
        public CharacterEntity TargetEntity { get; }

        /// <summary> スキル発動者です。 </summary>
        public CharacterEntity PlayerEntity { get; }

        /// <summary> 現在のビートタイプです。 </summary>
        public BeatType CurrentBeatType { get; }

        /// <summary> 解決済み対象一覧です。 </summary>
        public ReadOnlyMemory<CharacterEntity> TargetEntities { get; }
    }
}
