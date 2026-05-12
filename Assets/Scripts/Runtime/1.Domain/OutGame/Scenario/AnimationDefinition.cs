using System;

namespace KillChord.Runtime.Domain.OutGame.Scenario
{
    /// <summary>
    /// アニメーションを参照するための定義情報を保持する。
    /// </summary>
    public readonly struct AnimationDefinition
    {
        /// <summary>
        /// アニメーション定義を初期化する。
        /// </summary>
        public AnimationDefinition(string id, string assetKey)
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