using System;

namespace KillChord.Runtime.Domain.OutGame.Scenario
{
    /// <summary>
    /// 立ち絵を参照するための定義情報を保持する。
    /// </summary>
    public readonly struct PortraitDefinition
    {
        /// <summary>
        /// 立ち絵定義を初期化する。
        /// </summary>
        public PortraitDefinition(string id, string assetKey)
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

        /// <summary> Id を取得する。 </summary>
        public string Id { get; }
        /// <summary> AssetKey を取得する。 </summary>
        public string AssetKey { get; }
    }
}