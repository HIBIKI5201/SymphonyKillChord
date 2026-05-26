using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.OutGame.Scenario
{
    [CreateAssetMenu(
        fileName = "AnimationCatalogAsset",
        menuName = "KillChord/Runtime/Scenario/Animation Catalog")]
    /// <summary>
    /// Animation のカタログ情報を保持するアセット。
    /// </summary>
    public class AnimationCatalogAsset : ScriptableObject
    {
        /// <summary> Entries を取得する。 </summary>
        public IReadOnlyList<AnimationCatalogEntry> Entries => _entries;

        [SerializeField]
        private AnimationCatalogEntry[] _entries = Array.Empty<AnimationCatalogEntry>();
    }
}