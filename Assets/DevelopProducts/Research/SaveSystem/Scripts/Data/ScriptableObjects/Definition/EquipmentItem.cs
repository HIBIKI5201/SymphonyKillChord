using UnityEngine;
namespace Research.SaveSystem
{
    [CreateAssetMenu(fileName = "EquipmentItem", menuName = "Scriptable Objects/Symphony/EquipmentItem")]
    public class EquipmentItem : ScriptableObject
    {
        public int Id;
        public string Name;
        public string Description;
    }
}