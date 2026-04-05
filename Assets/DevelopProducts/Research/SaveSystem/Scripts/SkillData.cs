using UnityEngine;
namespace Research.SaveSystem
{ 
    public class SkillData : MonoBehaviour
    {
        public int Id => _id;

        [SerializeField] private int _id;
        [SerializeField] private string _name;
        [SerializeField] private string _description;
    }
}