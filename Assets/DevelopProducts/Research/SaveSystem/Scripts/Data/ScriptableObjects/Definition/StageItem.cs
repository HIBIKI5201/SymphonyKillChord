using UnityEngine;
namespace Research.SaveSystem
{
    [CreateAssetMenu(fileName = "StageItem", menuName = "Scriptable Objects/Symphony/StageItem")]
    public class StageItem : ScriptableObject
    {
        public int Id;
        public string Name;
        public string Description;
    }
}