using KillChord.Runtime.Domain.InGame.Character;
using System;

namespace KillChord.Runtime.Application.InGame.Skill
{
    /// <summary>
    ///     スキル対象解決の結果です。
    /// </summary>
    public readonly struct SkillTargetResolveResult
    {
        /// <summary>
        ///     解決結果を初期化します。
        /// </summary>
        /// <param name="primaryTargetEntity"> 主対象です。 </param>
        /// <param name="targetEntities"> 対象一覧です。 </param>
        public SkillTargetResolveResult(CharacterEntity primaryTargetEntity, CharacterEntity[] targetEntities)
        {
            PrimaryTargetEntity = primaryTargetEntity;
            TargetEntities = targetEntities ?? Array.Empty<CharacterEntity>();
        }

        /// <summary> 主対象です。 </summary>
        public CharacterEntity PrimaryTargetEntity { get; }

        /// <summary> 対象一覧です。 </summary>
        public ReadOnlyMemory<CharacterEntity> TargetEntities { get; }
    }
}
