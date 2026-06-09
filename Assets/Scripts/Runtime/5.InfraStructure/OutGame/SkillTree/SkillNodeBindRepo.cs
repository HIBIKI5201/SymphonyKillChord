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
        public SkillNodeBindData FindById(int id)
        {
            if(SkillNodeBinds == null || SkillNodeBinds.Length <= 0)
            {
                return null;
            }

            for (int i = 0; i < SkillNodeBinds.Length; i++)
            {
                if (SkillNodeBinds[i].SkillNodeData.NodeId == id)
                {
                    return SkillNodeBinds[i];
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
                if (SkillNodeBinds[i].NodeName == name)
                {
                    return SkillNodeBinds[i];
                }
            }
            return null;
        }
    }
}