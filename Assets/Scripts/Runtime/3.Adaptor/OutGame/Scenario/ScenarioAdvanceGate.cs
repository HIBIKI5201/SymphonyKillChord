using System;
using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Application;

namespace KillChord.Runtime.Adaptor
{
    public class ScenarioAdvanceGate : ITextAdvanceWaiter
    {
        public ValueTask WaitNextAsync(CancellationToken ct)
        {
            Task waitTask;
            lock (_sync)
            {
                if (_pending)
                {
                    _pending = false;
                    return default;
                }

                waitTask = _tcs.Task;
            }

            if (ct.IsCancellationRequested)
            {
                return new ValueTask(Task.FromCanceled(ct));
            }

            return new ValueTask(waitTask.WaitAsync(Timeout.InfiniteTimeSpan, TimeProvider.System, ct));
        }

        public void NotifyNext()
        {
            lock (_sync)
            {
                bool accepted = _tcs.TrySetResult(true);
                if (!accepted)
                {
                    _pending = true;
                }

                _tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            }
        }

        private TaskCompletionSource<bool> _tcs =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        private bool _pending;
        private readonly object _sync = new();
    }
}
