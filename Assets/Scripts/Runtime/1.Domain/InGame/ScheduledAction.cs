using System;
using System.Threading;
using UnityEngine;

namespace KillChord.Runtime.Domain
{
    public readonly struct ScheduledAction
    {
        public ScheduledAction(Action action, CancellationToken ct)
        {
            Action = action ?? throw new ArgumentNullException(nameof(action));
            CancellationToken = ct;
        }

        public Action Action { get; }
        public CancellationToken CancellationToken { get; }
    }
}