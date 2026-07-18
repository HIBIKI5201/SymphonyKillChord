using KillChord.Runtime.Application.OutGame.Scenario;
using KillChord.Runtime.Domain.OutGame.Scenario;

namespace KillChord.Runtime.InfraStructure.OutGame.Scenario
{
    /// <summary>
    /// Portrait の参照情報を取得するリポジトリ。
    /// </summary>
    public class PortraitRepository : CatalogRepositoryBase<PortraitId, PortraitDefinition, PortraitCatalogEntry>, IPortraitRepository
    {
        /// <summary>
        /// 立ち絵カタログから参照情報を構築する。
        /// </summary>
        public PortraitRepository(PortraitCatalogAsset catalog)
            : base(catalog != null ? catalog.Entries : null)
        {
        }

        /// <summary>
        /// カタログエントリから検索用の定義情報を生成する。
        /// </summary>
        protected override bool TryBuild(PortraitCatalogEntry entry, out PortraitId id, out PortraitDefinition definition)
        {
            if (entry.Id.Id == 0 || entry.Asset == null)
            {
                id = default;
                definition = default;
                return false;
            }

            id = new PortraitId(entry.Id.Id);
            string assetKey = string.IsNullOrWhiteSpace(entry.AssetKey) ? entry.Asset.name : entry.AssetKey;
            definition = new PortraitDefinition(id, assetKey);
            return true;
        }
    }
}
