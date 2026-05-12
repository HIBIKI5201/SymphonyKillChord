using KillChord.Runtime.Domain.OutGame.Scenario;

namespace KillChord.Runtime.Application.OutGame.Scenario
{
    /// <summary>
    /// 立ち絵定義の参照契約を定義します。
    /// </summary>
    public interface IPortraitRepository
    {
        bool TryFindById(string id, out PortraitDefinition portrait);
    }
}
