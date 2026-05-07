using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure
{
    [CreateAssetMenu(
        fileName = "BackgroundCatalogAsset",
        menuName = "KillChord/Runtime/Scenario/Background Catalog")]
    public class BackgroundCatalogAsset : ScriptableObject
    {
        public IReadOnlyList<BackgroundCatalogEntry> Entries => _entries;

        [SerializeField]
        private BackgroundCatalogEntry[] _entries = Array.Empty<BackgroundCatalogEntry>();
    }
}
