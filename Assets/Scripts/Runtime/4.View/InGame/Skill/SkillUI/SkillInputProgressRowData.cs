using System.Collections.Generic;

namespace KillChord.Runtime.View.InGame.Skill
{
    /// <summary>
    ///     スキル入力進行UIの1行分の表示データを管理するクラス。
    /// </summary>
    public class SkillInputProgressRowData
    {
        /// <summary>
        ///     スキル入力進行UIの1行分の表示データを生成する。
        /// </summary>
        /// <param name="skillId"> 対応するスキルのID。 </param>
        /// <param name="steps"> 1行分の拍子ごとの表示データのリスト。 </param>
        public SkillInputProgressRowData(int skillId, IReadOnlyList<SkillInputProgressStepData> steps)
        {
            SkillId = skillId;
            Steps = steps;
        }

        /// <summary> 対応するスキルのID。 </summary>
        public int SkillId { get; }

        /// <summary> 1行分の拍子ごとの表示データのリスト。 </summary>
        public IReadOnlyList<SkillInputProgressStepData> Steps { get; }
    }
}
