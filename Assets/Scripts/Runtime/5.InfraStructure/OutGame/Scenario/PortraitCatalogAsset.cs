using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.OutGame.Scenario
{
    [CreateAssetMenu(
        fileName = "PortraitCatalogAsset",
        menuName = "KillChord/Runtime/Scenario/Portrait Catalog")]
    /// <summary>
    /// Portrait のカタログ情報を保持するアセット。
    /// </summary>
    public class PortraitCatalogAsset : ScriptableObject
    {
        /// <summary> Entries を取得する。 </summary>
        public IReadOnlyList<PortraitCatalogEntry> Entries => _entries;

        [SerializeField]
        private PortraitCatalogEntry[] _entries = Array.Empty<PortraitCatalogEntry>();
    }
}