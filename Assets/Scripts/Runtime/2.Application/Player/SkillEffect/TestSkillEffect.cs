using KillChord.Runtime.Domain.Player;
using UnityEngine;

namespace KillChord.Runtime.Application.Player.SkillEffect
{
    /// <summary>
    ///     スキルの効果をテストするためのクラス。
    /// </summary>
    public class TestSkillEffect : ISkillEffect
    {
        public void Execute()
        {
            Debug.Log("SkillEffect Do");
        }
    }
}