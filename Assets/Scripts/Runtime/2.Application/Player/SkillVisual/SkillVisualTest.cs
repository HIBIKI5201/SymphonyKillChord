using KillChord.Runtime.Domain;
using UnityEngine;

namespace KillChord.Runtime.Application
{
    /// <summary>
    ///     スキルの視覚効果をテストするためのクラス。
    /// </summary>
    public class SkillVisualTest : ISkillVisual
    {
        public void Do()
        {
            Debug.Log("SkillVisual Do");
        }
    }
}