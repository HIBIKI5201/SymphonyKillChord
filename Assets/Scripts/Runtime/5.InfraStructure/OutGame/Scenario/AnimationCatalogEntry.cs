using System;
using KillChord.Runtime.Utility.Identity;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.OutGame.Scenario
{
    [Serializable]
    /// <summary>
    /// アニメーションカタログの 1 件分の参照情報を保持する。
    /// </summary>
    public struct AnimationCatalogEntry
    {
        [SourceDataCollection("ScenarioAnimation"), Tooltip("シナリオアニメーションを一意に識別するIDです。")]
        public DataID Id;

        [Tooltip("アニメーションのAddressableキーです。")]
        public string AssetKey;

        [Tooltip("再生するアニメーションクリップです。")]
        public AnimationClip Asset;
    }
}
