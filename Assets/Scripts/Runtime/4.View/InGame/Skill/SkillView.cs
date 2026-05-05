using KillChord.Runtime.Adaptor.InGame.Skill;
using UnityEngine;

namespace KillChord.Runtime.View
{
    public class SkillView : MonoBehaviour, ISkillVisual
    {
        public int Id => _id;
        public void Execute()
        {
            //実際のViewで起こる演出など
        }

        [SerializeField] private int _id;
    }
}
