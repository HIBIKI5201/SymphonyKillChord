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
        public IReadOnlyList<AnimationCatalogEntry> Entries => _entries;

        [SerializeField]
        private AnimationCatalogEntry[] _entries = Array.Empty<AnimationCatalogEntry>();
    }
}
