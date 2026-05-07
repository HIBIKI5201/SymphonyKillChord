using System;

namespace KillChord.Runtime.Application
{
    /// <summary>
    /// シナリオ再生設定の参照契約を定義します。
    /// </summary>
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
