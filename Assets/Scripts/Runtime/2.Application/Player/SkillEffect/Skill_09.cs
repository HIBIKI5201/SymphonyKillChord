using KillChord.Runtime.Domain;
using KillChord.Runtime.Domain.Player;
using UnityEngine;

namespace KillChord.Runtime.Application.Player.SkillEffect
{
    /// <summary>
    ///     スキルID 09 のスキル効果を実装するクラス。 
    /// </summary>
    public class Skill_09 : SkillBase
    {
        public Skill_09(IBuff buff) : base(buff)
        {
            
        }
        public override void Execute(SkillEffectContext context)
        {
            Debug.Log("TestSkillEffect executed!");
            // ここにスキルの効果を実装する
        }
    }
}