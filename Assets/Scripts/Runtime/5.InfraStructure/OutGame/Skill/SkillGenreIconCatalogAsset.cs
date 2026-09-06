using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.OutGame.Skill
{
    [CreateAssetMenu(
        fileName = "SkillGenreIconCatalogAsset",
        menuName = "KillChord/Runtime/Skill/Skill Genre Icon Catalog")]
    /// <summary>
    /// スキルジャンルアイコンのカタログ情報を保持するアセット。
    /// </summary>
    public class SkillGenreIconCatalogAsset : ScriptableObject
    {
        /// <summary> Entries を取得する。 </summary>
        public IReadOnlyList<SkillGenreIconCatalogEntry> Entries => _entries;

        [SerializeField]
        private SkillGenreIconCatalogEntry[] _entries = Array.Empty<SkillGenreIconCatalogEntry>();
    }
}
