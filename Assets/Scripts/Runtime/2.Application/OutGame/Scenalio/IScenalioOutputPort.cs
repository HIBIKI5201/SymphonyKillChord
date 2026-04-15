

using KillChord.Runtime.Domain;

namespace KillChord.Runtime.Application
{
    public interface IScenalioOutputPort
    {
        public void Present(IScenalioEvent senalioEvent);
    }
}
