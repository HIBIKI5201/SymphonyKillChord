using DevelopProducts.Architecture.Utility;
using UnityEngine;

namespace DevelopProducts.Architecture.InfraStructure
{
    [CreateAssetMenu(fileName = nameof(CharacterStatus),
        menuName = Const.CREATE_ASSET_PATH + nameof(CharacterStatus), order = 1)]
    public class CharacterStatus : ScriptableObject
    {
        public string Name => _name;
        public float Health => _health;
        public float Speed => _speed;

        [SerializeField]
        private string _name = "Character Name";
        [SerializeField, Min(0)]
        private float _health = 100;
        [SerializeField]
        private float _speed = 1;
    }
}
