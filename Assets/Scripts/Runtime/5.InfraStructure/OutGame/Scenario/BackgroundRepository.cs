using KillChord.Runtime.Application;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure
{
    public class BackgroundRepository : CatalogRepositoryBase<Sprite, BackgroundCatalogAsset.Entry>, IBackgroundRepository
    {
        public BackgroundRepository(BackgroundCatalogAsset catalog)
            : base(catalog != null ? catalog.Entries : null)
        {
        }

        protected override bool TryBuild(BackgroundCatalogAsset.Entry entry, out string id, out Sprite definition)
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
