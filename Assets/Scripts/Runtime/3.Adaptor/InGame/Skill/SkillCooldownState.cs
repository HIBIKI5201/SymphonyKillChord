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
        /// <summary>
        ///     スキル定義を指定して、すぐ使える状態で生成する。
        /// </summary>
        public SkillCooldownState(in SkillDefinition skill)
        {
            _skillDefinition = skill;
            _skillReadyTimestamp = 0f;
        }

        /// <summary> スキルを再び使えるようになる時刻。 </summary>
        public float SkillReadyTimestamp => _skillReadyTimestamp;

        /// <summary>
        ///     スキルのクールダウン完了時間を設定する。
        /// </summary>
        /// <param name="now"></param>
        public void SetSkillCooldown(float now)
        {
            _skillReadyTimestamp = now + (float)_skillDefinition.CooldownTime.Value;
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

        private readonly SkillDefinition _skillDefinition;
        private float _skillReadyTimestamp;
    }
}
