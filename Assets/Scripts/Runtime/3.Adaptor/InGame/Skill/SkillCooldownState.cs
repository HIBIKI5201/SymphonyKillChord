using KillChord.Runtime.Domain.InGame.Skill;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Skill
{
    /// <summary>
    ///     スキルごとのクールダウン完了時刻を管理し、スキルが発動可能かを判定するクラス。
    /// </summary>
    public class SkillCooldownState
    {
        public SkillCooldownState(in SkillDefinition skill)
        {
            _skillDefinition = skill;
            _skillReadyTimestamp = 0d;
        }

        /// <summary>
        ///     スキルのクールダウン完了時間を設定する。
        /// </summary>
        /// <param name="now"></param>
        public void SetSkillCooldown(float now)
        {
            _skillReadyTimestamp = now + _skillDefinition.CooldownTime.Value;
        }

        /// <summary>
        ///     スキル発動可能かどうかを判定する。
        /// </summary>
        /// <param name="now">現在時間</param>
        /// <returns></returns>
        public bool IsSkillReady(float now)
        {
            return now >= _skillReadyTimestamp;
        }

        private SkillDefinition _skillDefinition;
        // スキルのクールダウン完了時間を保持する
        private double _skillReadyTimestamp;
    }
}
