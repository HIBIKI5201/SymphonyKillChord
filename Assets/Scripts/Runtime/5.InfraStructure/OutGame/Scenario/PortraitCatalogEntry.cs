using System;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.OutGame.Scenario
{
    [Serializable]
    /// <summary>
    /// 立ち絵カタログの 1 件分の参照情報を保持する。
    /// </summary>
    public struct PortraitCatalogEntry
    {
        public string Id;
        public string AssetKey;
        public Sprite Asset;
    }
}