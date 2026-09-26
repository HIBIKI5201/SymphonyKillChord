using KillChord.Runtime.Domain.InGame.Skill;
using System;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.OutGame.Skill
{
    [Serializable]
    /// <summary>
    /// スキルジャンルアイコンカタログの 1 件分の参照情報を保持する。
    /// </summary>
    public struct SkillGenreIconCatalogEntry
    {
        [Tooltip("対応するスキルジャンルです。")]
        public SkillType Genre;

        [Tooltip("表示するアイコンです。")]
        public Sprite Icon;
    }
}
