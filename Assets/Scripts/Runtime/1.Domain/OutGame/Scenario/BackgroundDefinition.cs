using System;

namespace KillChord.Runtime.Domain
{
    public readonly struct BackgroundDefinition
    {
        public BackgroundDefinition(string id, string assetKey)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("id is empty.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(assetKey))
            {
                throw new ArgumentException("assetKey is empty.", nameof(assetKey));
            }

            Id = id;
            AssetKey = assetKey;
        }

        public string Id { get; }
        public string AssetKey { get; }
    }
}
