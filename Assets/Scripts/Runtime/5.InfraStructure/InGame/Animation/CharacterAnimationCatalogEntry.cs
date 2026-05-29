using KillChord.Runtime.Domain;
using System;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure
{
    /// <summary>
    ///     アニメーションカタログの1件分の参照情報を保持する。
    ///     InspectorでCharacterAnimationStateとAnimationClipをペアで設定する。
    /// </summary>
    [Serializable]
    public struct CharacterAnimationCatalogEntry
    {
        [Tooltip("アニメーション状態に対応するクリップ")]
        public CharacterAnimationState State;
        [Tooltip("対応するアニメーションクリップ")]
        public AnimationClip Clip;
    }
}
