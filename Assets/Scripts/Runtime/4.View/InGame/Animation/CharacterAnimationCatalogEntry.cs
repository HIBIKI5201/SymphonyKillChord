using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace KillChord.Runtime.View
{
    /// <summary>
    ///     アニメーションカタログの1件分の表示設定です。
    /// </summary>
    [Serializable]
    public struct CharacterAnimationCatalogEntry
    {
        [FormerlySerializedAs("State")]
        [Tooltip("アニメーション状態に対応するクリップです。")]
        public CharacterAnimationClipType ClipType;

        [Tooltip("ワンショット用の識別キーです。")]
        public string Key;

        [Tooltip("対応するアニメーションクリップです。")]
        public AnimationClip Clip;

        [Min(0)]
        [Tooltip("このクリップ専用の開始ブレンドフレーム数です。0の場合はConfig全体の既定値を使用します。")]
        public int EnterBlendFrameCount;

        [Min(0)]
        [Tooltip("このクリップ専用の終了ブレンドフレーム数です。0の場合はConfig全体の既定値を使用します。")]
        public int ExitBlendFrameCount;
    }
}
