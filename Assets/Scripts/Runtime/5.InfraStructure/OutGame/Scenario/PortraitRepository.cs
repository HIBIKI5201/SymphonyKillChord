using KillChord.Runtime.Application.OutGame.Scenario;
using KillChord.Runtime.Domain.OutGame.Scenario;

namespace KillChord.Runtime.InfraStructure.OutGame.Scenario
{
    public class PortraitRepository : CatalogRepositoryBase<PortraitDefinition, PortraitCatalogEntry>, IPortraitRepository
    {
        public PortraitRepository(PortraitCatalogAsset catalog)
            : base(catalog != null ? catalog.Entries : null)
        {
        }

        protected override bool TryBuild(PortraitCatalogEntry entry, out string id, out PortraitDefinition definition)
        {
            id = entry.Id;
            if (string.IsNullOrWhiteSpace(entry.Id) || entry.Asset == null)
            {
                definition = default;
                return false;
            }

            string assetKey = string.IsNullOrWhiteSpace(entry.AssetKey) ? entry.Asset.name : entry.AssetKey;
            definition = new PortraitDefinition(entry.Id, assetKey);
            return true;
        }
    }
}
