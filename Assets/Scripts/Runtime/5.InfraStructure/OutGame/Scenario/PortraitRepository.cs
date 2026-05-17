using KillChord.Runtime.Application.OutGame.Scenario;
using KillChord.Runtime.Domain.OutGame.Scenario;

namespace KillChord.Runtime.InfraStructure.OutGame.Scenario
{
    /// <summary>
    /// Portrait の参照情報を取得するリポジトリ。
    /// </summary>
    public class PortraitRepository : CatalogRepositoryBase<PortraitDefinition, PortraitCatalogEntry>, IPortraitRepository
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