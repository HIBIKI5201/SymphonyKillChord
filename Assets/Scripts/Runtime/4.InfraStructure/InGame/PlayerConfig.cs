using KillChord.Runtime.Domain;
using UnityEngine;

namespace KillChord.Structure
{
    [CreateAssetMenu(fileName = nameof(PlayerConfig), menuName = "KillChord/InGame/PlayerConfig")]
    public sealed class PlayerConfig : ScriptableObject
    {
        public PlayerMoveParameter ToDomain()
            => new(
                _moveSpeed,
                _dodgeSpeed,
                _dodgeDuration,
                _dodgeCooldown);

        [SerializeField] private float _moveSpeed;
        [Space]
        [SerializeField] private float _dodgeSpeed;
        [SerializeField] private float _dodgeDuration;
        [SerializeField] private float _dodgeCooldown;
    }
}
