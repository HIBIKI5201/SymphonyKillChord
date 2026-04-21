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
            if (_pending)
            {
                _pending = false;
                return new ValueTask();
            }
            if (ct.IsCancellationRequested)
            {
                return new ValueTask(Task.FromCanceled(ct));
            }

            return new ValueTask(_tcs.Task.WaitAsync(Timeout.InfiniteTimeSpan, TimeProvider.System, ct));
        }
        public void NotifyNext()
        {
            if (_tcs.Task.IsCanceled)
            {
                _pending = true;
                _tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                return;
            }
            _tcs.TrySetResult(true);
            _tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        }
        private TaskCompletionSource<bool> _tcs =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        private bool _pending;
    }
}
