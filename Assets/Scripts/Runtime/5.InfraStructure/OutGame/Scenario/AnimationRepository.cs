using KillChord.Runtime.Application.OutGame.Scenario;
using KillChord.Runtime.Domain.OutGame.Scenario;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.OutGame.Scenario
{
    /// <summary>
    /// Animation の参照情報を取得するリポジトリ。
    /// </summary>
    public class AnimationRepository : CatalogRepositoryBase<AnimationId, AnimationDefinition, AnimationCatalogEntry>, IAnimationRepository
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
        protected override bool TryBuild(AnimationCatalogEntry entry, out AnimationId id, out AnimationDefinition definition)
        {
            if (entry.Id.Id == 0 || entry.Asset == null)
            {
                id = default;
                definition = default;
                return false;
            }

            id = new AnimationId(entry.Id.Id);
            string assetKey = string.IsNullOrWhiteSpace(entry.AssetKey) ? entry.Asset.name : entry.AssetKey;
            definition = new AnimationDefinition(id, assetKey);
            return true;
        }
    }
}
