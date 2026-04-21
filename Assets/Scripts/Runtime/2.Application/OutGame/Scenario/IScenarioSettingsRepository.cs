using System;

namespace KillChord.Runtime.Application
{
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
