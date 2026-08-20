
using KillChord.Runtime.Domain.Player;
using UnityEngine;
using KillChord.Runtime.Domain.InGame.Buff;

namespace KillChord.Runtime.Application.Player.SkillEffect
{
    /// <summary>
    ///     スキルID 08 のスキル効果を実装するクラス。 
    /// </summary>
    public class Skill_08 : SkillBase
    {
        public Skill_08(IBuff buff) : base(buff)
        {
            
        }
        public override void Execute(in SkillEffectContext context)
        {
            Debug.Log("TestSkillEffect executed!");
            // ここにスキルの効果を実装する
        }
    }
}
