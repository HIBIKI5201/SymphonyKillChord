using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Player
{
    public interface IPlayerController
    {
        public bool TryDodge(Vector2 input, float time);
        public void Update(ref Quaternion rotation, Vector2 input, float time, out Vector3 velocity);
    }
}
