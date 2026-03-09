using UnityEngine;
namespace Research.Chou.OutGame
{
    [CreateAssetMenu(fileName = "EquipmentItemDB", menuName = "Scriptable Objects/Symphony/EquipmentItemDB")]
    public class EquipmentItemDB : ScriptableObject
    {
        public EquipmentItem[] Items;
    }
}