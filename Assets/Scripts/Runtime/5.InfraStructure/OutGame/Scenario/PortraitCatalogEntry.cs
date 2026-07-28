using System;
using KillChord.Runtime.Utility.Identity;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.OutGame.Scenario
{
    [Serializable]
    /// <summary>
    /// 立ち絵カタログの 1 件分の参照情報を保持する。
    /// </summary>
    public struct PortraitCatalogEntry
    {
        [SourceDataCollection("ScenarioPortrait"), Tooltip("シナリオ立ち絵を一意に識別するIDです。")]
        public DataID Id;

        [Tooltip("立ち絵のAddressableキーです。")]
        public string AssetKey;

        [Tooltip("表示する立ち絵です。")]
        public Sprite Asset;
    }
}
