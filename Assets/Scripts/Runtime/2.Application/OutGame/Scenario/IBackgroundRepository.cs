using KillChord.Runtime.Domain;

namespace KillChord.Runtime.Application
{
    /// <summary>
    /// 背景定義の参照契約を定義します。
    /// </summary>
    public interface IBackgroundRepository
    {
        bool TryFindById(string id, out BackgroundDefinition background);
    }
}
