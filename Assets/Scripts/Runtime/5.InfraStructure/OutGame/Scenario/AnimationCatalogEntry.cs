using System;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.OutGame.Scenario
{
    [Serializable]
    public struct AnimationCatalogEntry
    {
        public string Id;
        public string AssetKey;
        public AnimationClip Asset;
    }
}
