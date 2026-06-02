using KillChord.Runtime.Domain.InGame.Skill;
using System.Collections.Generic;

namespace KillChord.Runtime.Adaptor.InGame.Skill
{
    /// <summary>
    ///     スキルごとのクールダウン完了時刻を管理し、スキルが発動可能かを判定するクラス。
    /// </summary>
    public class SkillCooldownState
    {
        public SkillCooldownState(int[] skillIds)
        {
            _skillReadyTimestamp = new();
            if (skillIds != null && skillIds.Length > 0)
            {
                for (int i = 0; i < skillIds.Length; i++)
                {
                    _skillReadyTimestamp[skillIds[i]] = 0;
                }
            }
        }

        /// <summary>
        ///     発動したスキルにクールダウン完了時間を設定する。
        /// </summary>
        /// <param name="skill"></param>
        /// <param name="now"></param>
        public void SetSkillCooldown(in SkillDefinition skill, float now)
        {
            _skillReadyTimestamp[skill.Id.Value] = now + skill.CooldownTime.Value;
        }

        /// <summary>
        ///     スキル発動可能かどうかを判定する。
        /// </summary>
        /// <param name="skill">発動するスキル定義情報</param>
        /// <param name="now">現在時間</param>
        /// <returns></returns>
        public bool IsSkillReady(in SkillDefinition skill, float now)
        {
            // クールダウン完了時間が登録されていない場合、発動可能
            if (!_skillReadyTimestamp.ContainsKey(skill.Id.Value))
            {
                return true;
            }
            // 現在時間がクールダウン完了時間以降の場合、発動可能
            if (_skillReadyTimestamp[skill.Id.Value] <= now)
            {
                return true;
            }
            else
            {
                // クールダウン完了時間未到達の場合、発動不可
                return false;
            }
        }

        // スキルIDごとのクールダウン完了時間を保持する
        private Dictionary<int, double> _skillReadyTimestamp;
    }
}
