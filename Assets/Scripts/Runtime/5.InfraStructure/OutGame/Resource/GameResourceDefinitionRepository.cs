using KillChord.Runtime.Application.OutGame.Resource;
using KillChord.Runtime.Domain.OutGame.Resource;
using KillChord.Runtime.InfraStructure.Repository;
using KillChord.Runtime.Utility.Constant;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.OutGame.Resource
{
    /// <summary>
    ///     ゲーム内リソース定義アセットをIDで取得するリポジトリです。
    /// </summary>
    [CreateAssetMenu(
        fileName = nameof(GameResourceDefinitionRepository),
        menuName = PathConst.CREATE_ASSET_MENU_PATH + "Resource/" + nameof(GameResourceDefinitionRepository))]
    /// <summary>
    ///     ゲーム内リソースの定義を ID で検索するリポジトリ。
    /// </summary>
    public sealed class GameResourceDefinitionRepository
        : ScriptableObjectRepositoryBase<GameResourceId, GameResourceDefinitionAsset, GameResourceDefinitionAsset>,
            IGameResourceDefinitionRepository
    {
        /// <inheritdoc />
        public bool TryGetDefinition(GameResourceId id, out GameResourceDefinition definition)
        {
            if (!TryFind(id, out GameResourceDefinitionAsset asset))
            {
                definition = null;
                return false;
            }

            definition = asset.CreateDefinition();
            return true;
        }

        /// <summary>
        ///     指定したIDのリソース定義アセットを取得します。アイコンなどの表示用データの取得に使用します。
        /// </summary>
        /// <param name="id"> リソースIDです。 </param>
        /// <param name="asset"> 見つかったリソース定義アセットです。 </param>
        /// <returns> 見つかった場合はtrueです。 </returns>
        public bool TryGetAsset(GameResourceId id, out GameResourceDefinitionAsset asset)
        {
            return TryFind(id, out asset);
        }

        [SerializeField, Tooltip("IDで取得可能にするリソース定義アセットの一覧です。")]
        private GameResourceDefinitionAsset[] _resourceAssets;

        /// <inheritdoc />
        protected override IReadOnlyList<GameResourceDefinitionAsset> GetEntries() => _resourceAssets;

        /// <inheritdoc />
        protected override bool TryBuild(
            GameResourceDefinitionAsset entry,
            out GameResourceId id,
            out GameResourceDefinitionAsset value)
        {
            id = entry.Id;
            value = entry;
            return id.Value != 0;
        }
    }
}
