using UnityEngine;

namespace KillChord.Runtime.Adaptor
{
    public interface IController
    {
        public bool TryDodge(Vector2 input, float time);
        public void Update(ref Quaternion rotation, Vector2 input, float time, out Vector3 velocity);
    }
}
