using KillChord.Runtime.Application;
using KillChord.Runtime.Domain;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure
{
    public class AnimationRepository : CatalogRepositoryBase<AnimationDefinition, AnimationCatalogAsset.Entry>, IAnimationRepository
    {
        public AnimationRepository(AnimationCatalogAsset catalog)
            : base(catalog != null ? catalog.Entries : null)
        {
        }

        protected override bool TryBuild(AnimationCatalogAsset.Entry entry, out string id, out AnimationDefinition definition)
        {
            id = entry.Id;
            if (string.IsNullOrWhiteSpace(entry.Id) || entry.Asset == null)
            {
                definition = default;
                return false;
            }

            definition = new AnimationDefinition(entry.Id, entry.Id);
            return true;
        }
    }
}
