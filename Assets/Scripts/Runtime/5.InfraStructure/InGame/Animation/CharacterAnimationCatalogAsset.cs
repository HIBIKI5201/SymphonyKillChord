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

        /// <summary> ワンショット再生開始時のブレンドフレーム数です。 </summary>
        public int EnterBlendFrameCount => _enterBlendFrameCount;

        /// <summary> ワンショット再生終了時のブレンドフレーム数です。 </summary>
        public int ExitBlendFrameCount => _exitBlendFrameCount;

        [SerializeField, Tooltip("アニメーションクリップのカタログ。")]
        private CharacterAnimationCatalogEntry[] _entries = Array.Empty<CharacterAnimationCatalogEntry>();

        [SerializeField, Min(0), Tooltip("ワンショット再生開始時のブレンドフレーム数です。30FPS基準で扱います。")]
        private int _enterBlendFrameCount = 4;

        [SerializeField, Min(0), Tooltip("ワンショット再生終了時のブレンドフレーム数です。30FPS基準で扱います。")]
        private int _exitBlendFrameCount = 8;
    }
}
