using KillChord.Runtime.Domain.OutGame.SkillTree;
using KillChord.Runtime.Utility.Identity;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.OutGame.SkillTree
{
    /// <summary>
    ///     スキルノードに対応するデータを纏めたリポジトリー。
    /// </summary>
    [CreateAssetMenu(fileName = "SkillNodeBindRepo", menuName = "SymphonyDev/SkillTree/SkillNodeBindRepo")]
    public class SkillNodeBindRepo : ScriptableObject
    {
        public SkillNodeBindData[] SkillNodeBinds;

        /// <summary>
        ///     スキルノードのIDで対応するスキルノードデータを取得する。
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public SkillNodeBindData FindById(SkillNodeId id)
        {
            if(SkillNodeBinds == null || SkillNodeBinds.Length <= 0)
            {
                return null;
            }

            for (int i = 0; i < SkillNodeBinds.Length; i++)
            {
                var bind = SkillNodeBinds[i];
                if (bind == null || bind.SkillNodeData == null)
                {
                    continue;
                }
                if (bind.SkillNodeData.NodeId == id)
                {
                    return bind;
                }
            }
            return null;
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
    }
}
