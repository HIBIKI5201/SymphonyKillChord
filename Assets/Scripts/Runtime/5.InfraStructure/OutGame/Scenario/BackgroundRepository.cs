using System;
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
            if (string.IsNullOrWhiteSpace(entry.Id) || entry.Asset == null)
            {
                definition = default;
                return false;
            }

            definition = new BackgroundDefinition(entry.Id, entry.Id);
            return true;
        }
    }
}
