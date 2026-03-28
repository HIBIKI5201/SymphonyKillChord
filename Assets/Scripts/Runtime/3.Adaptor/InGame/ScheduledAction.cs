using System;
using System.Threading;
using UnityEngine;

namespace KillChord.Runtime.Adaptor
{
    public readonly struct ScheduledAction
    {
        public ScheduledAction(double time, Action action, CancellationToken ct)
        {
            Time = time;
            Action = action;
            CancellationToken = ct;
        }

        public double Time { get; }
        public Action Action { get; }
        public CancellationToken CancellationToken { get; }
    }
}
