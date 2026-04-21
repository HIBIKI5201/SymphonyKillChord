using KillChord.Runtime.Application.InGame.Mission;
using KillChord.Runtime.Domain.InGame.Mission;
using System.Collections.Generic;

namespace KillChord.Runtime.InfraStructure
{
    public class MissionDefinitionRepository : IMissionDefinitionRepository
    {
        public MissionDefinitionRepository(MissionCatalogAsset missionCatalogAsset)
        {
            _missionDefinitions = new Dictionary<MissionId, MissionDefinition>();

            MissionDefinitionAsset[] assets = missionCatalogAsset.MissionDefinitionAssets;

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

        public MissionDefinition Get(MissionId missionId)
        {
            return _missionDefinitions[missionId];
        }

        private readonly Dictionary<MissionId, MissionDefinition> _missionDefinitions = new();
    }
}
