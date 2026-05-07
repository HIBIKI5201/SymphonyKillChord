using KillChord.Runtime.Domain;

namespace KillChord.Runtime.Application
{
    /// <summary>
    /// 立ち絵定義の参照契約を定義します。
    /// </summary>
    public interface IPortraitRepository
    {
        bool TryFindById(string id, out PortraitDefinition portrait);
    }
}
