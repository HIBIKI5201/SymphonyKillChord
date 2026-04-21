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
                _missionDefinitions[definition.MissionId] = definition;
            }
        }

        public MissionDefinition Get(MissionId missionId)
        {
            return _missionDefinitions[missionId];
        }

        private readonly Dictionary<MissionId, MissionDefinition> _missionDefinitions = new();
    }
}
