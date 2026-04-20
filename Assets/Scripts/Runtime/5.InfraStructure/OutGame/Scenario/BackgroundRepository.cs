using KillChord.Runtime.Application;
using KillChord.Runtime.Domain;

namespace KillChord.Runtime.InfraStructure
{
    public class BackgroundRepository : CatalogRepositoryBase<BackgroundDefinition, BackgroundCatalogAsset.Entry>, IBackgroundRepository
    {
        public BackgroundRepository(BackgroundCatalogAsset catalog)
            : base(catalog != null ? catalog.Entries : null)
        {
        }

        protected override bool TryBuild(BackgroundCatalogAsset.Entry entry, out string id, out BackgroundDefinition definition)
        {
            id = entry.Id;
            if (string.IsNullOrWhiteSpace(entry.Id) || string.IsNullOrWhiteSpace(entry.AssetKey))
            {
                definition = default;
                return false;
            }

            definition = new BackgroundDefinition(entry.Id, entry.AssetKey);
            return true;
        }
    }
}
