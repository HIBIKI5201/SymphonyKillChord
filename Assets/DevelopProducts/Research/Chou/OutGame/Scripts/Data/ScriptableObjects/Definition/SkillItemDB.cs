using UnityEngine;
namespace Research.Chou.OutGame
{
    [CreateAssetMenu(fileName = "SkillItemDB", menuName = "Scriptable Objects/Symphony/SkillItemDB")]
    public class SkillItemDB : ScriptableObject
    {
        public SkillItem[] Items;
    }
}
