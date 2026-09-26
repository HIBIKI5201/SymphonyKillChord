using KillChord.Runtime.Domain.OutGame.SkillTree;
using KillChord.Runtime.InfraStructure.Repository;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.OutGame.SkillTree
{
    /// <summary>
    ///     スキルノードに対応するデータを纏めたリポジトリー。
    /// </summary>
    [CreateAssetMenu(fileName = "SkillNodeBindRepository", menuName = "SymphonyDev/SkillTree/SkillNodeBindRepository")]
    public class SkillNodeBindRepository : ScriptableObjectRepositoryBase<SkillNodeId, SkillNodeBindData, SkillNodeBindData>
    {
        public SkillNodeBindData[] SkillNodeBinds;

        /// <summary>
        ///     スキルノードのIDで対応するスキルノードデータを取得する。
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public SkillNodeBindData FindById(SkillNodeId id)
        {
            return TryFind(id, out SkillNodeBindData bind) ? bind : null;
        }

        /// <summary>
        ///     名前が一致するスキルノードの対応データを取得する。見つからない場合は null を返す。
        /// </summary>
        public SkillNodeBindData FindByName(string name)
        {
            if (SkillNodeBinds == null || SkillNodeBinds.Length <= 0)
            {
                return null;
            }

            for (int i = 0; i < SkillNodeBinds.Length; i++)
            {
                var bind = SkillNodeBinds[i];
                if (bind == null)
                {
                    continue;
                }
                if (bind.NodeName == name)
                {
                    return bind;
                }
            }
            return null;
        }

        /// <inheritdoc/>
        protected override IReadOnlyList<SkillNodeBindData> GetEntries() => SkillNodeBinds;

        /// <inheritdoc/>
        protected override bool TryBuild(SkillNodeBindData entry, out SkillNodeId id, out SkillNodeBindData value)
        {
            value = entry;
            id = entry.SkillNodeId;
            return true;
        }
    }
}
