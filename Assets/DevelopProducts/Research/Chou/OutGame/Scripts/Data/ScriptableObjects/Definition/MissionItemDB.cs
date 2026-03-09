using UnityEngine;
namespace Research.Chou.OutGame
{
    [CreateAssetMenu(fileName = "MissionItemDB", menuName = "Scriptable Objects/MissionItemDB")]
    public class MissionItemDB : ScriptableObject
    {
        public MissionItem[] Items;
    }
}