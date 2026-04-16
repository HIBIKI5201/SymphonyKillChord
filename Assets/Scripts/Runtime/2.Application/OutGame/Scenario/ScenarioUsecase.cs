
using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Domain;

namespace KillChord.Runtime.Application
{
    public class ScenarioUsecase
    {
        public ScenarioUsecase(IScenarioRepository repo, ScenarioHandlerRepo handlerRepo)
        {
            _scenarioRepo = repo;
            _handleRepo = handlerRepo;
        }

        public async ValueTask PlayScenario()
        {
            ScenarioData data = _scenarioRepo.FindById("test");
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;
            foreach (IScenarioEvent e in data.Events)
            {
                IScenarioEventHandler handler = _handleRepo.FindById(e.GetType());
                if (handler == null) continue;
                await handler.HandleAsync(e, token);
            }

        }
        private readonly ScenarioHandlerRepo _handleRepo;
        private readonly IScenarioRepository _scenarioRepo;
    }
}
