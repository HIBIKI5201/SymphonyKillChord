using KillChord.Runtime.Application.OutGame.Scenario;
using KillChord.Runtime.Domain.OutGame.Scenario;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.OutGame.Scenario
{
    public class AnimationRepository : CatalogRepositoryBase<AnimationDefinition, AnimationCatalogEntry>, IAnimationRepository
    {
        public AnimationRepository(AnimationCatalogAsset catalog)
            : base(catalog != null ? catalog.Entries : null)
        {
        }

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
