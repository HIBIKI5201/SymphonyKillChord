using System;

namespace KillChord.Runtime.Application.OutGame.Scenario
{
    /// <summary>
    /// IScenarioSettings の参照情報を取得するリポジトリ。
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