using UnityEngine;
namespace Research.Chou.OutGame
{
    [CreateAssetMenu(fileName = "MissionItem", menuName = "Scriptable Objects/Symphony/MissionItem")]
    public class MissionItem : ScriptableObject
    {
        public int Id;
        public string Name;
        public string Description;
    }
}