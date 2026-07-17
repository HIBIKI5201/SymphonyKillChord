using System;
using KillChord.Runtime.Application.OutGame.Scenario;
using KillChord.Runtime.Domain.OutGame.Scenario;

namespace KillChord.Runtime.InfraStructure.OutGame.Scenario
{
    /// <summary>
    /// Background の参照情報を取得するリポジトリ。
    /// </summary>
    public class BackgroundRepository : CatalogRepositoryBase<BackgroundId, BackgroundDefinition, BackgroundCatalogEntry>, IBackgroundRepository
    {
        /// <summary>
        /// 背景カタログから参照情報を構築する。
        /// </summary>
        public BackgroundRepository(BackgroundCatalogAsset catalog)
            : base(catalog != null ? catalog.Entries : null)
        {
        }

        /// <summary>
        /// カタログエントリから検索用の定義情報を生成する。
        /// </summary>
        protected override bool TryBuild(BackgroundCatalogEntry entry, out BackgroundId id, out BackgroundDefinition definition)
        {
            if (entry.Id.Id == 0 || entry.Asset == null)
            {
                id = default;
                definition = default;
                return false;
            }

            id = new BackgroundId(entry.Id.Id);
            string assetKey = string.IsNullOrWhiteSpace(entry.AssetKey) ? entry.Asset.name : entry.AssetKey;
            definition = new BackgroundDefinition(id, assetKey);
            return true;
        }
    }
}
