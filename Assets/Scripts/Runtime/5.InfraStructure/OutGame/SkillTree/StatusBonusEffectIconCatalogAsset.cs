using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.OutGame.SkillTree
{
    [CreateAssetMenu(
        fileName = "StatusBonusEffectIconCatalogAsset",
        menuName = "KillChord/Runtime/SkillTree/Status Bonus Effect Icon Catalog")]
    /// <summary>
    /// ステータスボーナス効果アイコンのカタログ情報を保持するアセット。
    /// </summary>
    public class StatusBonusEffectIconCatalogAsset : ScriptableObject
    {
        /// <summary> Entries を取得する。 </summary>
        public IReadOnlyList<StatusBonusEffectIconCatalogEntry> Entries => _entries;

        [SerializeField]
        private StatusBonusEffectIconCatalogEntry[] _entries = Array.Empty<StatusBonusEffectIconCatalogEntry>();
    }
}
