using UnityEngine;
namespace Research.SaveSystem
{
    [CreateAssetMenu(fileName = "SkillItemDB", menuName = "Scriptable Objects/Symphony/SkillItemDB")]
    public class SkillItemDB : ScriptableObject
    {
        public SkillItem[] Items;
    }
}
