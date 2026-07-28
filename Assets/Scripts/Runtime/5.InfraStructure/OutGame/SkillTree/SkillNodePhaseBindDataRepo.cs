using KillChord.Runtime.Domain.OutGame.SkillTree;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.OutGame.SkillTree
{
    /// <summary>
    ///     解放段階と必要スキルノードの紐づきを纏めたリポジトリー。
    /// </summary>
    [CreateAssetMenu(fileName = "SkillNodePhaseBindDataRepo", menuName = "SymphonyDev/SkillTree/SkillNodePhaseBindDataRepo")]
    public class SkillNodePhaseBindDataRepo : ScriptableObject
    {
        public SkillNodePhaseBindData[] PhaseBindData;

        /// <summary>
        ///     スキルノードIDを指定し、段階を解放できるかチェックし、実行する。
        /// </summary>
        /// <param name="unlockedNodeId"></param>
        /// <param name="phaseName"></param>
        /// <returns></returns>
        public bool TryGetUnlockPhaseName(SkillNodeId unlockedNodeId, out string phaseName)
        {
            if (PhaseBindData == null || PhaseBindData.Length == 0)
            {
                phaseName = null;
                return false;
            }

            for (int i = 0; i < PhaseBindData.Length; i++)
            {
                var bind = PhaseBindData[i];
                if (bind == null)
                {
                    continue;
                }
                if (bind.RequiredSkillNodeId == unlockedNodeId)
                {
                    phaseName = bind.PhaseName;
                    return true;
                }
            }
            phaseName = null;
            return false;
        }
    }
}
