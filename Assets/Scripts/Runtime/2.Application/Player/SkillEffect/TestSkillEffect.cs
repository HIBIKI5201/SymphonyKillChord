using KillChord.Runtime.Domain;
using KillChord.Runtime.Domain.InGame.Buff;
using KillChord.Runtime.Domain.Player;
using UnityEngine;

namespace KillChord.Runtime.Application.Player.SkillEffect
{
    /// <summary>
    ///     スキルの効果をテストするためのクラス。
    /// </summary>
    public class TestSkillEffect : SkillBase
    {
        public TestSkillEffect(IBuff buff) : base(buff)
        {
            
        }
        public override void Execute(SkillEffectContext context)
        {
            Debug.Log("TestSkillEffect executed!");
            // ここにスキルの効果を実装する
        }
    }
}
