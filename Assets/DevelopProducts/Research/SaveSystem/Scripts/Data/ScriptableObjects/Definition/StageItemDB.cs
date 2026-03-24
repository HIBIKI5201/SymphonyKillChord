using UnityEngine;
namespace Research.SaveSystem
{
    [CreateAssetMenu(fileName = "StageItemDB", menuName = "Scriptable Objects/Symphony/StageItemDB")]
    public class StageItemDB : ScriptableObject
    {
        public StageItem[] Items;
    }
}