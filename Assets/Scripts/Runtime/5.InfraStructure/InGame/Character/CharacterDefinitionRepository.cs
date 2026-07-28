using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.InfraStructure.Repository;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Character
{
    /// <summary>
    ///     キャラクター定義アセットをIDで検索するリポジトリです。
    /// </summary>
    [CreateAssetMenu(
        fileName = nameof(CharacterDefinitionRepository),
        menuName = "KillChord/Character/" + nameof(CharacterDefinitionRepository))]
    public sealed class CharacterDefinitionRepository
        : ScriptableObjectRepositoryBase<CharacterDefinitionId, CharacterDefinitionAsset, CharacterDefinitionAsset>
    {
        /// <summary>
        ///     IDに対応するキャラクター定義アセットを取得しようとします。
        /// </summary>
        /// <param name="id"> 取得するキャラクター定義IDです。 </param>
        /// <param name="asset"> 取得したキャラクター定義アセットです。 </param>
        /// <returns> IDに対応するアセットが存在する場合はtrueです。 </returns>
        public bool TryGetAsset(CharacterDefinitionId id, out CharacterDefinitionAsset asset)
        {
            return TryFind(id, out asset);
        }

        [SerializeField, Tooltip("IDで取得可能にするキャラクター定義アセットの一覧です。")]
        private CharacterDefinitionAsset[] _characterAssets;

        protected override IReadOnlyList<CharacterDefinitionAsset> GetEntries() => _characterAssets;

        protected override bool TryBuild(
            CharacterDefinitionAsset entry,
            out CharacterDefinitionId id,
            out CharacterDefinitionAsset value)
        {
            id = entry.Id;
            value = entry;
            return true;
        }
    }
}
