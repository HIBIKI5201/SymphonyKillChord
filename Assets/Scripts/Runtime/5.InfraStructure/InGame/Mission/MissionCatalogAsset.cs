using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Mission
{
    [CreateAssetMenu(fileName = nameof(MissionCatalogAsset)
        , menuName = "KillChord/Mission" + "/" + nameof(MissionCatalogAsset))]
    public class MissionCatalogAsset : ScriptableObject
    {
        public MissionDefinitionAsset[] MissionDefinitionAssets => _missionDefinitionAssets;

        [SerializeField] private MissionDefinitionAsset[] _missionDefinitionAssets;
    }
}
