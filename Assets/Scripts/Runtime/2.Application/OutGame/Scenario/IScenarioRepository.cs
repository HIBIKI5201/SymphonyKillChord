using KillChord.Runtime.Domain;

namespace KillChord.Runtime.Application
{
    public interface IScenarioRepository
    {
        ScenarioData FindById(string id);
    }
}
