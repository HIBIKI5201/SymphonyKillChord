using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Application;
using KillChord.Runtime.Domain;
using KillChord.Runtime.View;
using UnityEngine;

namespace KillChord.Runtime.Composition
{
    public sealed class PlayerInitializer : MonoBehaviour
    {
        [SerializeField] private Player _player;
        private void Awake()
        {
            PlayerMovement playerMovement = new(new MoveSpeed(10), new MoveSpeed(10), 0.3f);
            PlayerController playerMovementController = new(playerMovement);
            _player.Init(playerMovementController);
        }
    }
}
