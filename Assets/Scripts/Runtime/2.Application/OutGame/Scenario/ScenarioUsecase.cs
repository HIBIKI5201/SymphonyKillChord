
using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Domain;

namespace KillChord.Runtime.Application
{
    public class ScenarioUsecase
    {
        public ScenarioUsecase(IScenarioRepository repo, ScenarioHandlerRepo handlerRepo, ITextAdvanceWaiter textAdvanceWaiter)
        {
            _scenarioRepo = repo;
            _handleRepo = handlerRepo;
            _textAdvanceWaiter = textAdvanceWaiter;
        }

        public async ValueTask PlayScenario()
        {
            ScenarioData data = _scenarioRepo.FindById("test");
            using CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;
            foreach (IScenarioEvent e in data.Events)
            {
                await _handleRepo.HandleAsync(e, token);
                if (e.RequirePlayerAdvance)
                {
                    await _textAdvanceWaiter.WaitNextAsync(token);
                }
            }

        }
        private readonly ITextAdvanceWaiter _textAdvanceWaiter;
        private readonly ScenarioHandlerRepo _handleRepo;
        private readonly IScenarioRepository _scenarioRepo;
    }
}
