using KillChord.Runtime.Domain.OutGame.Resource;

namespace KillChord.Runtime.Application.OutGame.Resource
{
    /// <summary>
    ///     ゲーム内リソースの定義をIDから取得するリポジトリです。
    /// </summary>
    public interface IGameResourceDefinitionRepository
    {
        /// <summary>
        ///     指定したIDのリソース定義を取得します。
        /// </summary>
        /// <param name="id"> リソースIDです。 </param>
        /// <param name="definition"> 見つかったリソース定義です。 </param>
        /// <returns> 見つかった場合はtrueです。 </returns>
        bool TryGetDefinition(GameResourceId id, out GameResourceDefinition definition);
    }
}
