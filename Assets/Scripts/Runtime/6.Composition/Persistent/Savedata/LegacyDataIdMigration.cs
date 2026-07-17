using KillChord.Runtime.Domain.Persistent.Savedata;
using KillChord.Runtime.Utility.Identity;
using System;
using System.Collections.Generic;

namespace KillChord.Runtime.Composition.Persistent.Savedata
{
    /// <summary>
    ///     ID統一前の連番IDを現在のハッシュIDへ移行します。
    /// </summary>
    internal static class LegacyDataIdMigration
    {
        private const string SKILL_CATEGORY = "Skill";
        private const string SKILL_NODE_CATEGORY = "SkillNode";
        private const string STAGE_CATEGORY = "Stage";

        private static readonly IReadOnlyDictionary<int, int> LEGACY_SKILL_ID_MAP = CreateLegacySkillIdMap();
        private static readonly IReadOnlyDictionary<int, int> LEGACY_SKILL_NODE_ID_MAP = CreateLegacySkillNodeIdMap();
        private static readonly IReadOnlyDictionary<int, int> LEGACY_STAGE_ID_MAP = CreateLegacyStageIdMap();

        /// <summary>
        ///     保存データに含まれる旧IDを統一IDへ変換します。
        /// </summary>
        /// <param name="saveData"> 移行対象の保存データ。 </param>
        /// <returns> 保存内容を変更した場合はtrue。 </returns>
        public static bool TryMigrate(SaveData saveData)
        {
            if (saveData == null)
            {
                throw new ArgumentNullException(nameof(saveData));
            }

            bool isChanged = false;

            if (TryRemapIds(saveData.SkillUnlock.UnlockedSkillIds, LEGACY_SKILL_ID_MAP, out int[] skillIds))
            {
                saveData.SkillUnlock.SetUnlockedSkillIds(skillIds);
                isChanged = true;
            }

            if (TryRemapIds(saveData.SkillUnlock.UnlockedSkillNodeIds, LEGACY_SKILL_NODE_ID_MAP, out int[] skillNodeIds))
            {
                saveData.SkillUnlock.SetUnlockedSkillNodeIds(skillNodeIds);
                isChanged = true;
            }

            if (TryRemapIds(saveData.SkillBuild.EquipmentSkillIDs, LEGACY_SKILL_ID_MAP, out int[] equipmentSkillIds))
            {
                saveData.SkillBuild.SetEquipmentSkillIDs(new List<int>(equipmentSkillIds));
                isChanged = true;
            }

            return saveData.StageProgress.RemapStageIds(LEGACY_STAGE_ID_MAP) || isChanged;
        }

        /// <summary>
        ///     ID一覧を対応表に従って置換し、変換後に重複するIDを除去します。
        /// </summary>
        /// <param name="sourceIds"> 変換前ID一覧。 </param>
        /// <param name="idMap"> 置換前IDと置換後IDの対応表。 </param>
        /// <param name="migratedIds"> 変換後ID一覧。 </param>
        /// <returns> 一覧を変更した場合はtrue。 </returns>
        private static bool TryRemapIds(
            IReadOnlyList<int> sourceIds,
            IReadOnlyDictionary<int, int> idMap,
            out int[] migratedIds)
        {
            if (sourceIds == null || sourceIds.Count == 0)
            {
                migratedIds = Array.Empty<int>();
                return false;
            }

            List<int> result = new List<int>(sourceIds.Count);
            HashSet<int> uniqueIds = new HashSet<int>();
            bool isChanged = false;

            for (int i = 0; i < sourceIds.Count; i++)
            {
                int sourceId = sourceIds[i];
                int migratedId = idMap.TryGetValue(sourceId, out int replacementId)
                    ? replacementId
                    : sourceId;
                isChanged |= migratedId != sourceId;

                if (!uniqueIds.Add(migratedId))
                {
                    isChanged = true;
                    continue;
                }

                result.Add(migratedId);
            }

            migratedIds = result.ToArray();
            return isChanged;
        }

        /// <summary>
        ///     旧スキルIDの対応表を生成します。
        /// </summary>
        /// <returns> 旧スキルIDと統一IDの対応表。 </returns>
        private static IReadOnlyDictionary<int, int> CreateLegacySkillIdMap()
        {
            return new Dictionary<int, int>
            {
                { 0, DataIDHasher.Compute(SKILL_CATEGORY, "skill_00") },
                { 1, DataIDHasher.Compute(SKILL_CATEGORY, "skill_01") },
                { 2, DataIDHasher.Compute(SKILL_CATEGORY, "skill_02") },
                { 3, DataIDHasher.Compute(SKILL_CATEGORY, "skill_03") },
                { 13, DataIDHasher.Compute(SKILL_CATEGORY, "skill_13") }
            };
        }

        /// <summary>
        ///     旧スキルノードIDの対応表を生成します。
        /// </summary>
        /// <returns> 旧スキルノードIDと統一IDの対応表。 </returns>
        private static IReadOnlyDictionary<int, int> CreateLegacySkillNodeIdMap()
        {
            Dictionary<int, int> idMap = new Dictionary<int, int>();
            AddSkillNodeRange(idMap, 1, 11);
            AddSkillNodeRange(idMap, 101, 137);
            return idMap;
        }

        /// <summary>
        ///     指定範囲の旧スキルノードIDを対応表へ追加します。
        /// </summary>
        /// <param name="idMap"> 追加先の対応表。 </param>
        /// <param name="startId"> 範囲の開始ID。 </param>
        /// <param name="endId"> 範囲の終了ID。 </param>
        private static void AddSkillNodeRange(IDictionary<int, int> idMap, int startId, int endId)
        {
            for (int id = startId; id <= endId; id++)
            {
                idMap.Add(id, DataIDHasher.Compute(SKILL_NODE_CATEGORY, $"node_n{id}"));
            }
        }

        /// <summary>
        ///     旧ステージIDの対応表を生成します。
        /// </summary>
        /// <returns> 旧ステージIDと統一IDの対応表。 </returns>
        private static IReadOnlyDictionary<int, int> CreateLegacyStageIdMap()
        {
            return new Dictionary<int, int>
            {
                { 1, DataIDHasher.Compute(STAGE_CATEGORY, "stage_tutorial") },
                { 2, DataIDHasher.Compute(STAGE_CATEGORY, "stage_02") },
                { 3, DataIDHasher.Compute(STAGE_CATEGORY, "stage_03") },
                { 4, DataIDHasher.Compute(STAGE_CATEGORY, "stage_04") },
                { 5, DataIDHasher.Compute(STAGE_CATEGORY, "stage_05") },
                { 6, DataIDHasher.Compute(STAGE_CATEGORY, "stage_06") }
            };
        }
    }
}
