using KillChord.Runtime.Domain.InGame;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame
{
    public sealed class TargetManagerController
    {
        public TargetManagerController(ITargetRegisteable targetRegisteable)
        {
            _targetRegisteable = targetRegisteable;
        }

        public void Register(Transform target)
        {
            _targetRegisteable.Register(target);
        }
        public void Unregister(Transform target)
        {
            _targetRegisteable.Unregister(target);
        }

        private readonly ITargetRegisteable _targetRegisteable;
    }
}
