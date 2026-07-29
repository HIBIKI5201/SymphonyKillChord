using KillChord.Runtime.Domain.InGame.Mission;
using KillChord.Runtime.InfraStructure.Repository;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Mission
{
    /// <summary>
    ///     ミッション定義アセットをIDで検索するリポジトリです。
    /// </summary>
    [CreateAssetMenu(
        fileName = nameof(MissionDefinitionRepository),
        menuName = "KillChord/Mission/" + nameof(MissionDefinitionRepository))]
    public sealed class MissionDefinitionRepository
        : ScriptableObjectRepositoryBase<MissionId, MissionDefinitionAsset, MissionDefinitionAsset>, IMissionPreviewProvider
    {
        /// <summary>
        ///     指定されたミッションIDに対応するミッション定義アセットを取得しようとします。
        /// </summary>
        /// <param name="id"> 取得するミッションIDです。 </param>
        /// <param name="asset"> 取得したミッション定義アセットです。 </param>
        /// <returns> IDに対応するアセットが存在する場合はtrueです。 </returns>
        public bool TryGetAsset(MissionId id, out MissionDefinitionAsset asset)
        {
            return TryFind(id, out asset);
        }

        /// <inheritdoc />
        public bool TryGetPreview(
            MissionId missionId,
            out string mainMissionText,
            out IReadOnlyList<string> evaluationDescriptions)
        {
            if (TryFind(missionId, out MissionDefinitionAsset asset))
            {
                mainMissionText = asset.MainMissionText;
                evaluationDescriptions = asset.GetEvaluationDescriptions();
                return true;
            }

            mainMissionText = null;
            evaluationDescriptions = null;
            return false;
        }

        /// <summary>
        ///     指定されたミッションIDに対応するミッション定義を生成しようとします。
        /// </summary>
        /// <param name="id"> 取得するミッションIDです。 </param>
        /// <param name="missionKeyRepository"> 敵ミッションキーの解決に使うリポジトリです。 </param>
        /// <param name="missionDefinition"> 生成したミッション定義です。 </param>
        /// <returns> IDに対応するアセットが存在する場合はtrueです。 </returns>
        public bool TryCreateMissionDefinition(
            MissionId id,
            EnemyMissionKeyRepository missionKeyRepository,
            out MissionDefinition missionDefinition)
        {
            if (TryFind(id, out MissionDefinitionAsset asset))
            {
                missionDefinition = asset.Create(missionKeyRepository);
                return true;
            }

            missionDefinition = null;
            return false;
        }

        [SerializeField, Tooltip("プランナーが編集可能にするミッション定義アセットの一覧です。")]
        private MissionDefinitionAsset[] _missionDefinitionAssets;

        protected override IReadOnlyList<MissionDefinitionAsset> GetEntries() => _missionDefinitionAssets;

        protected override bool TryBuild(
            MissionDefinitionAsset entry,
            out MissionId id,
            out MissionDefinitionAsset value)
        {
            id = entry.Id;
            value = entry;
            return true;
        }
    }
}
