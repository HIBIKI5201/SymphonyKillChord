

namespace KillChord.Runtime.Domain
{
    public interface IScenarioRepository
    {
        public ScenarioData FindById(string id);
    }
}
