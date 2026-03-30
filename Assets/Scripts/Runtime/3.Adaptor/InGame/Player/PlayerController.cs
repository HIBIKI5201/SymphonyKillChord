using KillChord.Runtime.Application.InGame.Player;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Player
{
    public class PlayerController
    {
        public PlayerController(PlayerApplication playerApplication)
        {
            _playerApplication = playerApplication;
        }

        public bool TryDodge(Vector2 input, float time)
            => _playerApplication.TryDodge(input, time);
        public void Update(ref Quaternion rotation, Vector2 input, float time, out Vector3 velocity)
        {
            _playerApplication.Update(ref rotation, input, time, out velocity);
        }

        private readonly PlayerApplication _playerApplication;

    }
}
