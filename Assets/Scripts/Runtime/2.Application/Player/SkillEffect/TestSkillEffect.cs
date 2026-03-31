using KillChord.Runtime.Domain;
using UnityEngine;

namespace KillChord.Runtime.Application
{
    public class TestSkillEffect : ISkillEffect
    {
        public void Do()
        {
            Debug.Log("SkillEffect Do");
        }
    }
}