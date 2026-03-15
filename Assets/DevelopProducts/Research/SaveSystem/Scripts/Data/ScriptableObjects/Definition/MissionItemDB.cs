using UnityEngine;
namespace Research.SaveSystem
{
    [CreateAssetMenu(fileName = "MissionItemDB", menuName = "Scriptable Objects/Symphony/MissionItemDB")]
    public class MissionItemDB : ScriptableObject
    {
        public MissionItem[] Items;
    }
}