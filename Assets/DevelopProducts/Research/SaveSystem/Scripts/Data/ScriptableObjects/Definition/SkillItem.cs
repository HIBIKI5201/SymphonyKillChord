using UnityEngine;

namespace Research.SaveSystem
{
    [CreateAssetMenu(fileName = "SkillItem", menuName = "Scriptable Objects/Symphony/SkillItem")]
    public class SkillItem : ScriptableObject
    {
        public int Id;
        public string Name;
        public string Description;
    }
}