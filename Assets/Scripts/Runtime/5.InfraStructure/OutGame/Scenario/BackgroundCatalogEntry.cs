using System;
using KillChord.Runtime.Utility.Identity;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.OutGame.Scenario
{
    [Serializable]
    /// <summary>
    /// 背景カタログの 1 件分の参照情報を保持する。
    /// </summary>
    public struct BackgroundCatalogEntry
    {
        [SourceDataCollection("ScenarioBackground"), Tooltip("シナリオ背景を一意に識別するIDです。")]
        public DataID Id;

        [Tooltip("背景のAddressableキーです。")]
        public string AssetKey;

        [Tooltip("表示する背景です。")]
        public Sprite Asset;
    }
}
