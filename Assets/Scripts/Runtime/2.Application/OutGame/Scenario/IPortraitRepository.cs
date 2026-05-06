using KillChord.Runtime.Domain;

namespace KillChord.Runtime.Application
{
    public interface IPortraitRepository
    {
        bool TryFindById(string id, out PortraitDefinition portrait);
    }
}
