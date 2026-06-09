using UnityEngine;

namespace KillChord.Runtime.InfraStructure.OutGame.SkillTree
{
    /// <summary>
    ///     スキルノードデータを纏めたリポジトリー。
    /// </summary>
    [CreateAssetMenu(fileName = "SkillNodeDataRepo", menuName = "SymphonyDev/SkillTree/SkillNodeDataRepo")]
    public class SkillNodeDataRepo : ScriptableObject
    {
        public SkillNodeData[] SkillNodes;

        /// <summary>
        ///     スキルノードのIDを指定して、スキルノードデータを取得する。
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public SkillNodeData FindNodeData(int id)
        {
            if (SkillNodes == null || SkillNodes.Length <= 0)
            {
                Debug.LogError($"[SkillNodeDataRepo] スキルノード情報リポジトリーが空です。");
            }
            for(int i = 0; i < SkillNodes.Length; i++)
            {
                if (SkillNodes[i].NodeId == id)
                {
                    return SkillNodes[i];
                }
            }
            Debug.LogError($"[SkillNodeDataRepo] 指定されてスキルノードIDが見つかりません。");
            return null;
        }

    }
}