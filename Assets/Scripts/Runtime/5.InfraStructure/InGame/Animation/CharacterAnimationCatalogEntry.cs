using KillChord.Runtime.View;
using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace KillChord.Runtime.InfraStructure
{
    /// <summary>
     ///     アニメーションカタログの1件分の参照情報を保持する。
    ///     Inspectorで基本クリップ種別またはワンショットキーとAnimationClipを対応付ける。
    /// </summary>
    [Serializable]
    public struct CharacterAnimationCatalogEntry
    {
        [FormerlySerializedAs("State")]
        [Tooltip("アニメーション状態に対応するクリップ")]
        public CharacterAnimationClipType ClipType;
        [Tooltip("ワンショット用の識別キー")]
        public string Key;
        [Tooltip("対応するアニメーションクリップ")]
        public AnimationClip Clip;
    }
}
