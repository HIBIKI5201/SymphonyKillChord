using UnityEngine;

namespace KillChord.Runtime.Domain.InGame
{
    public interface ITargetRegisteable
    {
        public void Register(Transform target);
        public void Unregister(Transform target);
    }
}
