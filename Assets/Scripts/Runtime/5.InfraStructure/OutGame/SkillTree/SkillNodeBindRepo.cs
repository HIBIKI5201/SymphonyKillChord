using KillChord.Runtime.Domain.OutGame.SkillTree;
using KillChord.Runtime.InfraStructure.Repository;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.OutGame.SkillTree
{
    /// <summary>
    ///     スキルノードに対応するデータを纏めたリポジトリー。
    /// </summary>
    [CreateAssetMenu(fileName = "SkillNodeBindRepo", menuName = "SymphonyDev/SkillTree/SkillNodeBindRepo")]
    public class SkillNodeBindRepo : ScriptableObjectRepositoryBase<SkillNodeId, SkillNodeBindData, SkillNodeBindData>
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

        protected override IReadOnlyList<SkillNodeBindData> GetEntries() => SkillNodeBinds;

        protected override bool TryBuild(SkillNodeBindData entry, out SkillNodeId id, out SkillNodeBindData value)
        {
            value = entry;
            id = entry.SkillNodeId;
            return true;
        }
    }
}
