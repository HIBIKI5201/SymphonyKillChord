using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure
{
    /// <summary>
    ///     キャラクターアニメーションクリップのカタログを保持するScriptableObject。
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterAnimationCatalog", menuName = "KillChord/CharacterAnimationCatalog")]
    public sealed class CharacterAnimationCatalogAsset : ScriptableObject
    {
        /// <summary> カタログエントリ一覧。 </summary>
        public IReadOnlyList<CharacterAnimationCatalogEntry> Entries => _entries;

        [SerializeField, Tooltip("アニメーションクリップのカタログ。")]
        private CharacterAnimationCatalogEntry[] _entries = Array.Empty<CharacterAnimationCatalogEntry>();
    }
}
