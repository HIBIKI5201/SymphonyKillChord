using KillChord.Runtime.Domain.OutGame.Scenario;

namespace KillChord.Runtime.Application.OutGame.Scenario
{
    /// <summary>
    /// 背景定義の参照契約を定義します。
    /// </summary>
    public interface IBackgroundRepository
    {
        bool TryFindById(string id, out BackgroundDefinition background);
    }
}
