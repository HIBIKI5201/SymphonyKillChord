using UnityEngine;

namespace DevelopProducts.Architecture.InfraStructure
{
    [CreateAssetMenu(fileName = nameof(CharacterStatus),
        menuName = "DevelopProducts/" + nameof(CharacterStatus), order = 1)]
    public class CharacterStatus : ScriptableObject
    {
        public string Name => _name;
        public float Health => _health;

        [SerializeField]
        private string _name = "Character Name";
        [SerializeField, Min(0)]
        private float _health = 100;
    }
}
