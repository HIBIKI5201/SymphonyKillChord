using KillChord.Runtime.Domain.OutGame.SkillTree;
using System;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.OutGame.SkillTree
{
    [Serializable]
    /// <summary>
    /// ステータスボーナス効果アイコンカタログの 1 件分の参照情報を保持する。
    /// </summary>
    public struct StatusBonusEffectIconCatalogEntry
    {
        [Tooltip("対応するステータスボーナス効果の種別です。")]
        public StatusBonusEffectKind Kind;

        [Tooltip("表示するアイコンです。")]
        public Sprite Icon;
    }
}
