using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.OutGame.Scenario
{
    [CreateAssetMenu(
        fileName = "BackgroundCatalogAsset",
        menuName = "KillChord/Runtime/Scenario/Background Catalog")]
    /// <summary>
    /// Background のカタログ情報を保持するアセット。
    /// </summary>
    public class BackgroundCatalogAsset : ScriptableObject
    {
        /// <summary> Entries を取得する。 </summary>
        public IReadOnlyList<BackgroundCatalogEntry> Entries => _entries;

        [SerializeField]
        private BackgroundCatalogEntry[] _entries = Array.Empty<BackgroundCatalogEntry>();
    }
}