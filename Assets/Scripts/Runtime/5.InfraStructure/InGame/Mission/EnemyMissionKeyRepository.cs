using KillChord.Runtime.Domain.InGame.Mission;
using KillChord.Runtime.InfraStructure.Repository;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Mission
{
    /// <summary>
    ///     敵のミッションキーアセットをIDで検索するリポジトリです。
    /// </summary>
    [CreateAssetMenu(
        fileName = nameof(EnemyMissionKeyRepository),
        menuName = "KillChord/Mission/" + nameof(EnemyMissionKeyRepository))]
    public sealed class EnemyMissionKeyRepository
        : ScriptableObjectRepositoryBase<EnemyMissionKey, EnemyMissionKeyAsset, EnemyMissionKeyAsset>
    {
        /// <summary>
        ///     IDに対応する敵ミッションキーアセットを取得しようとします。
        /// </summary>
        /// <param name="id"> 取得する敵ミッションキーIDです。 </param>
        /// <param name="asset"> 取得した敵ミッションキーアセットです。 </param>
        /// <returns> IDに対応するアセットが存在する場合はtrueです。 </returns>
        public bool TryGetAsset(EnemyMissionKey id, out EnemyMissionKeyAsset asset)
        {
            return TryFind(id, out asset);
        }

        [SerializeField, Tooltip("IDで取得可能にする敵ミッションキーアセットの一覧です。")]
        private EnemyMissionKeyAsset[] _missionKeyAssets;

        protected override IReadOnlyList<EnemyMissionKeyAsset> GetEntries() => _missionKeyAssets;

        protected override bool TryBuild(
            EnemyMissionKeyAsset entry,
            out EnemyMissionKey id,
            out EnemyMissionKeyAsset value)
        {
            id = entry.Id;
            value = entry;
            return true;
        }
    }
}
