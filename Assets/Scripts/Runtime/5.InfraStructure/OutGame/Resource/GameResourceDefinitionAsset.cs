using KillChord.Runtime.Domain.OutGame.Resource;
using KillChord.Runtime.Utility.Constant;
using KillChord.Runtime.Utility.Identity;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.OutGame.Resource
{
    /// <summary>
    ///     ゲーム内リソース（研究ポイント・改造ポイント・強化素材など）の定義を保持するアセットです。
    /// </summary>
    [CreateAssetMenu(
        fileName = nameof(GameResourceDefinitionAsset),
        menuName = PathConst.CREATE_ASSET_MENU_PATH + "Resource/" + nameof(GameResourceDefinitionAsset))]
    /// <summary>
    ///     ゲーム内リソースを定義するデータ。
    /// </summary>
    public sealed class GameResourceDefinitionAsset : ScriptableObject
    {
        /// <summary> リソースのIDです。 </summary>
        public GameResourceId Id => new GameResourceId(_id.Id);

        /// <summary> 画面に表示するリソース名です。 </summary>
        public string DisplayName => _displayName;

        /// <summary> 画面に表示するアイコンです。 </summary>
        public Sprite Icon => _icon;

        /// <summary> リソースの説明文です。 </summary>
        public string Description => _description;

        /// <summary>
        ///     リソース定義を生成します。
        /// </summary>
        /// <returns> 生成したリソース定義です。 </returns>
        public GameResourceDefinition CreateDefinition()
        {
            return new GameResourceDefinition(Id, _displayName);
        }

        [SerializeField, SourceDataCollection(GameResourceIds.COLLECTION_KEY)]
        [Tooltip("リソースを一意に識別するID。研究ポイントは \"ResearchPoint\"、改造ポイントは \"SkillLevelupPoint\" を入力する。")]
        private DataID _id;

        [SerializeField, Tooltip("画面に表示するリソース名。")]
        private string _displayName;

        [SerializeField, Tooltip("画面に表示するアイコン。")]
        private Sprite _icon;

        [SerializeField, TextArea, Tooltip("リソースの説明文。")]
        private string _description;
    }
}
