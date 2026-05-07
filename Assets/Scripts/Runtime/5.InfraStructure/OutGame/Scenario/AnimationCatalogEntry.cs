using System;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure
{
    [Serializable]
    public struct AnimationCatalogEntry
    {
        public string Id;
        public string AssetKey;
        public AnimationClip Asset;
    }
}
