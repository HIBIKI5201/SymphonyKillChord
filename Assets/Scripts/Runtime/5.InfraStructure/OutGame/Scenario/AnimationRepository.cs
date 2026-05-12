using KillChord.Runtime.Application.OutGame.Scenario;
using KillChord.Runtime.Domain.OutGame.Scenario;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.OutGame.Scenario
{
    /// <summary>
    /// Animation の参照情報を取得するリポジトリ。
    /// </summary>
    public class AnimationRepository : CatalogRepositoryBase<AnimationDefinition, AnimationCatalogEntry>, IAnimationRepository
    {
        /// <summary>
        /// アニメーションカタログから参照情報を構築する。
        /// </summary>
        public AnimationRepository(AnimationCatalogAsset catalog)
            : base(catalog != null ? catalog.Entries : null)
        {
        }

        /// <summary>
        /// カタログエントリから検索用の定義情報を生成する。
        /// </summary>
        protected override bool TryBuild(AnimationCatalogEntry entry, out string id, out AnimationDefinition definition)
        {
            id = entry.Id;
            if (string.IsNullOrWhiteSpace(entry.Id) || entry.Asset == null)
            {
                definition = default;
                return false;
            }

            string assetKey = string.IsNullOrWhiteSpace(entry.AssetKey) ? entry.Asset.name : entry.AssetKey;
            definition = new AnimationDefinition(entry.Id, assetKey);
            return true;
        }
    }
}