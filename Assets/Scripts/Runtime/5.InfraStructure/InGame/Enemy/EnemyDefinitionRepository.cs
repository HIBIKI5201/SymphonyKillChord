using KillChord.Runtime.Domain.InGame.Enemy;
using KillChord.Runtime.InfraStructure.Repository;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Enemy
{
    /// <summary>
    ///     個別の敵定義アセットをIDで検索するリポジトリです。
    /// </summary>
    [CreateAssetMenu(
        fileName = nameof(EnemyDefinitionRepository),
        menuName = "KillChord/Enemy/" + nameof(EnemyDefinitionRepository))]
    public sealed class EnemyDefinitionRepository
        : ScriptableObjectRepositoryBase<EnemyDefinitionId, EnemyDefinitionAsset, EnemyDefinitionAsset>
    {
        /// <summary> 登録済みの敵定義一覧です。 </summary>
        public IReadOnlyList<EnemyDefinitionAsset> Definitions =>
            _enemyDefinitionAssets ?? System.Array.Empty<EnemyDefinitionAsset>();

        /// <summary>
        ///     IDに対応する敵定義を取得しようとします。
        /// </summary>
        /// <param name="id"> 取得する敵定義IDです。 </param>
        /// <param name="definition"> 取得した敵定義です。 </param>
        /// <returns> 対応する敵定義が存在する場合はtrueです。 </returns>
        public bool TryGetDefinition(EnemyDefinitionId id, out EnemyDefinitionAsset definition)
        {
            return TryFind(id, out definition);
        }

        [SerializeField, Tooltip("IDで取得可能にする個別の敵定義アセット一覧です。")]
        private EnemyDefinitionAsset[] _enemyDefinitionAssets;

        /// <summary>
        ///     登録済みの敵定義一覧を取得します。
        /// </summary>
        /// <returns> 敵定義一覧です。 </returns>
        protected override IReadOnlyList<EnemyDefinitionAsset> GetEntries()
        {
            return _enemyDefinitionAssets;
        }

        /// <summary>
        ///     リポジトリエントリからIDと敵定義を構築します。
        /// </summary>
        /// <param name="entry"> 敵定義アセットです。 </param>
        /// <param name="id"> 敵定義IDです。 </param>
        /// <param name="value"> 敵定義です。 </param>
        /// <returns> 有効なIDとプレハブを持つ場合はtrueです。 </returns>
        protected override bool TryBuild(
            EnemyDefinitionAsset entry,
            out EnemyDefinitionId id,
            out EnemyDefinitionAsset value)
        {
            id = entry.Id;
            value = entry;
            if (id.Value != 0
                && entry.ViewPrefab != null
                && entry.CharacterDefinition != null
                && entry.MoveSpec != null
                && entry.EncounterMusicSpec != null
                && entry.BattleMusicSpec != null
                && entry.MissionKey != null)
            {
                return true;
            }

            Debug.LogWarning(
                $"[{nameof(EnemyDefinitionRepository)}] 敵定義の必須データが不足しています。 Asset: {entry.name}",
                this);
            return false;
        }
    }
}
