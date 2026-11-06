using KillChord.Runtime.Domain.Player;
using UnityEngine;

namespace KillChord.Runtime.Application.Player.SkillEffect
{
    /// <summary>
    ///     スキルの効果をテストするためのクラス。
    /// </summary>
    public class TestSkillEffect : SkillBase
    {
        public override void Execute(in SkillEffectContext context)
        {
            Debug.Log("TestSkillEffect executed!");
            // ここにスキルの効果を実装する
        }
    }
}
