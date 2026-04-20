using KillChord.Runtime.Application;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure
{
    public class AnimationRepository : CatalogRepositoryBase<AnimationClip, AnimationCatalogAsset.Entry>, IAnimationRepository
    {
        public AnimationRepository(AnimationCatalogAsset catalog)
            : base(catalog != null ? catalog.Entries : null)
        {
        }

        protected override bool TryBuild(AnimationCatalogAsset.Entry entry, out string id, out AnimationClip definition)
        {
            id = entry.Id;
            if (string.IsNullOrWhiteSpace(entry.Id) || entry.Asset == null)
            {
                definition = default;
                return false;
            }

            definition = entry.Asset;
            return true;
        }
    }
}
