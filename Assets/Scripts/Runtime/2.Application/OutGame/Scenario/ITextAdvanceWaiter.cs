
using System;
using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Application
{
    public interface ITextAdvanceWaiter
    {
        public ValueTask WaitNextAsync(CancellationToken ct);
    }

    public interface IScenarioCompletionNotifier
    {
        ValueTask NotifyCompletedAsync(bool skipped, CancellationToken ct);
    }

    public interface IScenarioSettingsRepository
    {
        TimeSpan NormalTextCharInterval { get; }
        TimeSpan FastForwardTextCharInterval { get; }
        TimeSpan PausePollInterval { get; }
        TimeSpan CloseDelayAfterComplete { get; }
        bool SkipClosesImmediately { get; }
        bool WaitForInputOnLastText { get; }
        string DefaultScenarioId { get; }
    }
}
