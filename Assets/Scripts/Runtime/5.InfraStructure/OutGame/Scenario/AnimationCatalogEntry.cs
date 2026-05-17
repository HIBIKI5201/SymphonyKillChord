using System;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.OutGame.Scenario
{
    [Serializable]
    /// <summary>
    /// アニメーションカタログの 1 件分の参照情報を保持する。
    /// </summary>
    public struct AnimationCatalogEntry
    {
        public string Id;
        public string AssetKey;
        public AnimationClip Asset;
    }
}