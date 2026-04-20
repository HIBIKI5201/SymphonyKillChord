using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure
{
    [CreateAssetMenu(
        fileName = "AnimationCatalogAsset",
        menuName = "KillChord/Runtime/Scenario/Animation Catalog")]
    public class AnimationCatalogAsset : ScriptableObject
    {
        public IReadOnlyList<Entry> Entries => _entries;

        [SerializeField]
        private Entry[] _entries = Array.Empty<Entry>();

        [Serializable]
        public struct Entry
        {
            public string Id;
            public string AssetKey;
        }
    }
}
