using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure
{
    [CreateAssetMenu(
        fileName = "PortraitCatalogAsset",
        menuName = "KillChord/Runtime/Scenario/Portrait Catalog")]
    public class PortraitCatalogAsset : ScriptableObject
    {
        public IReadOnlyList<Entry> Entries => _entries;

        [SerializeField]
        private Entry[] _entries = Array.Empty<Entry>();

        [Serializable]
        public struct Entry
        {
            public string Id;
            public string AssetKey;
            public Sprite Asset;
        }
    }
}
