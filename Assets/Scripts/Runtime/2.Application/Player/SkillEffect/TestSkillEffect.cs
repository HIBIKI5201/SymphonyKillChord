using KillChord.Runtime.Domain;
using UnityEngine;

namespace KillChord.Runtime.Application
{
    /// <summary>
    ///     スキルの効果をテストするためのクラス。
    /// </summary>
    public class TestSkillEffect : ISkillEffect
    {
        public void Do()
        {
            Debug.Log("SkillEffect Do");
        }
    }
}