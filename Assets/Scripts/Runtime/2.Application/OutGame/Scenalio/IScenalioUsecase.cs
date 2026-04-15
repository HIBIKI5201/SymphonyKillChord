using KillChord.Runtime.Domain;


namespace KillChord.Runtime.Application
{
    public interface IScenalioUsecase
    {
        public void Start(ScenalioData data);
        public void NotifyCompleted();
    }
}
