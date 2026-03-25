using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Application;
using KillChord.Runtime.Domain;
using KillChord.Runtime.View;
using UnityEngine;

namespace KillChord.Runtime.Composition
{
    public sealed class PlayerInitializer : MonoBehaviour
    {
        [SerializeField] private PlayerView _player;
        private void Awake()
        {
            PlayerDodgeMovementApplication dodge = new(new MoveSpeed(20), 0.2f, 0.3f);
            PlayerMovement move = new(new MoveSpeed(10));
            PlayerApplication application = new(move, dodge);

            PlayerController playerMovementController = new(application);
            _player.Init(playerMovementController);
        }
    }
}
