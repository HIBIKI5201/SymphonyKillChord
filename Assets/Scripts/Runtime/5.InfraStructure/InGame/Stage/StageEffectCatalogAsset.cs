using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Stage
{
    /// <summary>
    ///     ステージ演出IDの一覧を保持するカタログアセットです。
    ///     <para> SourceDataProvider の Collection「StageEffect」の登録先として使います。 </para>
    /// </summary>
    [CreateAssetMenu(
        fileName = "StageEffectCatalogAsset",
        menuName = "KillChord/Runtime/InGame/Stage Effect Catalog")]
    public class StageEffectCatalogAsset : ScriptableObject
    {
        /// <summary> 登録されている演出の一覧です。 </summary>
        public IReadOnlyList<StageEffectCatalogEntry> Entries => _entries;

        [SerializeField, Tooltip("ステージ演出IDの登録一覧です。")]
        private StageEffectCatalogEntry[] _entries = Array.Empty<StageEffectCatalogEntry>();
    }
}
