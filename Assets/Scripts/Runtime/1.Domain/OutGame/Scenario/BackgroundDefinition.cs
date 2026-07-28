using System;

namespace KillChord.Runtime.Domain.OutGame.Scenario
{
    /// <summary>
    /// 背景を参照するための定義情報を保持する。
    /// </summary>
    public readonly struct BackgroundDefinition
    {
        /// <summary>
        /// 背景定義を初期化する。
        /// </summary>
        public BackgroundDefinition(BackgroundId id, string assetKey)
        {
            if (string.IsNullOrWhiteSpace(assetKey))
            {
                throw new ArgumentException("assetKey is empty.", nameof(assetKey));
            }

            Id = id;
            AssetKey = assetKey;
        }

        /// <summary> Id を取得する。 </summary>
        public BackgroundId Id { get; }
        /// <summary> AssetKey を取得する。 </summary>
        public string AssetKey { get; }
    }
}
