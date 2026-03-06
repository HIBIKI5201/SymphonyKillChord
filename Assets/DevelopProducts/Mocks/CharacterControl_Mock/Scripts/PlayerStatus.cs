using UnityEngine;

namespace Mock.CharacterControl
{
    [CreateAssetMenu(fileName = nameof(PlayerStatus), menuName = "Mock/CharacterControl/" + nameof(PlayerStatus))]
    public class PlayerStatus : ScriptableObject
    {
        public float MoveSpeed => _moveSpeed;

        [SerializeField]
        private float _moveSpeed = 3;
    }
}
