using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.Domain.InGame.Skill;
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
        /// <param name="effectSpec"> 発動するスキルの効果定義です。 </param>
        public SkillEffectContext(
            CharacterEntity targetEntity,
            CharacterEntity playerEntity,
            BeatType currentBeatType,
            ReadOnlyMemory<CharacterEntity> targetEntities,
            SkillEffectSpec effectSpec,
            bool isJustHit)
        {
            TargetEntity = targetEntity;
            PlayerEntity = playerEntity;
            CurrentBeatType = currentBeatType;
            TargetEntities = targetEntities;
            EffectSpec = effectSpec;
            IsJustHit = isJustHit;
        }

        /// <summary> 被スキル者です。 </summary>
        public CharacterEntity TargetEntity { get; }

        /// <summary> スキル発動者です。 </summary>
        public CharacterEntity PlayerEntity { get; }

        /// <summary> 現在のビートタイプです。 </summary>
        public BeatType CurrentBeatType { get; }

        /// <summary> 解決済み対象一覧です。 </summary>
        public ReadOnlyMemory<CharacterEntity> TargetEntities { get; }

        /// <summary> 発動するスキルの効果定義です。 </summary>
        public SkillEffectSpec EffectSpec { get; }

        /// <summary> ジャスト入力によるスキル発動かどうか。 </summary>
        public bool IsJustHit { get; }
    }
}
