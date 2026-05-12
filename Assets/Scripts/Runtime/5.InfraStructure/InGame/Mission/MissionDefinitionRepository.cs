using KillChord.Runtime.Application.InGame.Mission;
using KillChord.Runtime.Domain.InGame.Mission;
using System.Collections.Generic;

namespace KillChord.Runtime.InfraStructure.InGame.Mission
{
    /// <summary>
    ///     ミッション定義を管理するリポジトリクラス。
    /// </summary>
    public class MissionDefinitionRepository : IMissionDefinitionRepository
    {
        /// <summary>
        ///     MissionDefinitionRepository クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="missionCatalogAsset">ミッションカタログアセット。</param>
        public MissionDefinitionRepository(MissionCatalogAsset missionCatalogAsset)
        {
            _missionDefinitions = new Dictionary<MissionId, MissionDefinition>();

            if (missionCatalogAsset == null)
            {
                throw new System.ArgumentNullException(nameof(missionCatalogAsset));
            }

            MissionDefinitionAsset[] assets = missionCatalogAsset.MissionDefinitionAssets;

            if (assets == null)
            {
                throw new System.InvalidOperationException(
                "MissionDefinitionRepository requires MissionCatalogAsset.MissionDefinitionAssets.");
            }

            for (int i = 0; i < assets.Length; i++)
            {
                if (assets[i] == null)
                {
                    continue;
                }

                MissionDefinition definition = assets[i].Create();
                if (_missionDefinitions.ContainsKey(definition.MissionId))
                {
                    throw new System.InvalidOperationException(
                    $"Duplicate MissionId is registered: {definition.MissionId}");
                }

                _missionDefinitions.Add(definition.MissionId, definition);
            }
        }

        /// <summary>
        ///     指定したIDのミッション定義を取得します。
        /// </summary>
        /// <param name="missionId">ミッションID。</param>
        /// <returns>ミッション定義。</returns>
        public MissionDefinition Get(MissionId missionId)
        {
            if (_missionDefinitions.TryGetValue(missionId, out MissionDefinition definition))
            {
                return definition;
            }

            throw new System.InvalidOperationException(
            $"MissionDefinition is not registered: {missionId}");
        }

        private readonly Dictionary<MissionId, MissionDefinition> _missionDefinitions = new();
    }
}
