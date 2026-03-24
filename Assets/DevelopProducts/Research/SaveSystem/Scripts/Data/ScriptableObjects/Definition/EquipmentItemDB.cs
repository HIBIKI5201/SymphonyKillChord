using UnityEngine;
namespace Research.SaveSystem
{
    [CreateAssetMenu(fileName = "EquipmentItemDB", menuName = "Scriptable Objects/Symphony/EquipmentItemDB")]
    public class EquipmentItemDB : ScriptableObject
    {
        public EquipmentItem[] Items;
    }
}