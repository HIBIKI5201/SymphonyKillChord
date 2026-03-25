using KillChord.Runtime.Application;
using UnityEngine;

namespace KillChord.Runtime.Adaptor
{
    public class PlayerController
    {
        public PlayerController(PlayerApplication playerApplication)
        {
            _playerApplication = playerApplication;
        }

        public bool TryDodge(Vector2 input, float time)
            => _playerApplication.TryDodge(input, time);
        public void Update(ref Vector3 position, ref Quaternion rotation, Vector2 input, float time, float deltaTime)
        {
            _playerApplication.Update(ref position, ref rotation, input, time, deltaTime);
        }

        private readonly PlayerApplication _playerApplication;
    }
}
