using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.View
{
    /// <summary>
    ///     キャラクターアニメーションクリップの表示設定です。
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterAnimationConfig", menuName = "KillChord/View/Character Animation Config")]
    public sealed class CharacterAnimationCatalogConfig : ScriptableObject
    {
        /// <summary> カタログエントリ一覧です。 </summary>
        public IReadOnlyList<CharacterAnimationCatalogEntry> Entries => _entries;

        /// <summary> ワンショット再生開始時のブレンドフレーム数です。 </summary>
        public int EnterBlendFrameCount => _enterBlendFrameCount;

        /// <summary> ワンショット再生終了時のブレンドフレーム数です。 </summary>
        public int ExitBlendFrameCount => _exitBlendFrameCount;

        [SerializeField, Tooltip("アニメーションクリップのカタログです。")]
        private CharacterAnimationCatalogEntry[] _entries = Array.Empty<CharacterAnimationCatalogEntry>();

        [SerializeField, Min(0), Tooltip("ワンショット再生開始時のブレンドフレーム数です。30FPS基準で扱います。")]
        private int _enterBlendFrameCount = 4;

        [SerializeField, Min(0), Tooltip("ワンショット再生終了時のブレンドフレーム数です。30FPS基準で扱います。")]
        private int _exitBlendFrameCount = 8;
    }
}
