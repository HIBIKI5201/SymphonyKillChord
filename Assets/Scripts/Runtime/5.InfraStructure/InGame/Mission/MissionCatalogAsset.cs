using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Mission
{
    /// <summary>
    ///     ミッション定義アセットをまとめるカタログクラス。
    /// </summary>
    [CreateAssetMenu(fileName = nameof(MissionCatalogAsset)
        , menuName = "KillChord/Mission" + "/" + nameof(MissionCatalogAsset))]
    public class MissionCatalogAsset : ScriptableObject
    {
        /// <summary> ミッション定義アセットのリストを取得します。 </summary>
        public MissionDefinitionAsset[] MissionDefinitionAssets => _missionDefinitionAssets;

        [SerializeField, Tooltip("カタログに含めるミッション定義アセットのリスト。")] private MissionDefinitionAsset[] _missionDefinitionAssets;
    }
}
