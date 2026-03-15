using UnityEngine;
namespace Research.SaveSystem
{
    [CreateAssetMenu(fileName = "MissionItemDB", menuName = "Scriptable Objects/MissionItemDB")]
    public class MissionItemDB : ScriptableObject
    {
        public MissionItem[] Items;
    }
}