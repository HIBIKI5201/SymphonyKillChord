using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.OutGame.Scenario
{
    [CreateAssetMenu(
        fileName = "PortraitCatalogAsset",
        menuName = "KillChord/Runtime/Scenario/Portrait Catalog")]
    public class PortraitCatalogAsset : ScriptableObject
    {
        public IReadOnlyList<PortraitCatalogEntry> Entries => _entries;

        [SerializeField]
        private PortraitCatalogEntry[] _entries = Array.Empty<PortraitCatalogEntry>();
    }
}
