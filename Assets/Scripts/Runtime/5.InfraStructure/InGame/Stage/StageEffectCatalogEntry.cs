using System;
using KillChord.Runtime.Utility.Identity;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Stage
{
    /// <summary>
    ///     ステージ演出カタログの 1 件分の情報を保持します。
    /// </summary>
    [Serializable]
    public struct StageEffectCatalogEntry
    {
        [SourceDataCollection("StageEffect"), Tooltip("ステージ演出を一意に識別するIDです。")]
        public DataID Id;

        [Tooltip("演出の内容を説明するメモです。")]
        public string Description;
    }
}
