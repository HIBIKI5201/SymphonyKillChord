using System;
using KillChord.Runtime.Application.OutGame.Scenario;
using KillChord.Runtime.Domain.OutGame.Scenario;

namespace KillChord.Runtime.InfraStructure.OutGame.Scenario
{
    /// <summary>
    /// Background の参照情報を取得するリポジトリ。
    /// </summary>
    public class BackgroundRepository : CatalogRepositoryBase<BackgroundDefinition, BackgroundCatalogEntry>, IBackgroundRepository
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
        protected override bool TryBuild(BackgroundCatalogEntry entry, out string id, out BackgroundDefinition definition)
        {
            id = entry.Id;
            if (string.IsNullOrWhiteSpace(entry.Id) || entry.Asset == null)
            {
                definition = default;
                return false;
            }

            string assetKey = string.IsNullOrWhiteSpace(entry.AssetKey) ? entry.Asset.name : entry.AssetKey;
            definition = new BackgroundDefinition(entry.Id, assetKey);
            return true;
        }
    }
}