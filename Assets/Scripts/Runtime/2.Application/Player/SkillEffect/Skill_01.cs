using KillChord.Runtime.Domain.Player;
using UnityEngine;

namespace KillChord.Runtime.Application.Player.SkillEffect
{
    /// <summary>
    ///     スキルの効果をテストするためのクラス。
    /// </summary>
    public class Skill_01 : ISkillEffect
    {
        public void Execute(SkillEffectContext context)
        {
            Debug.Log("Skill_01 executed!");
            // ここにスキルの効果を実装する
        }
    }
}