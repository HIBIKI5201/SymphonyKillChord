using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Domain;

namespace KillChord.Runtime.Application
{
    public class ScenarioUsecase : IScenarioEventEmitter
    {
        public ScenarioUsecase(IScenarioRepository repo, ScenarioHandlerRepo handlerRepo, ITextAdvanceWaiter textAdvanceWaiter)
        {
            _scenarioRepo = repo;
            _handlerRepo = handlerRepo;
            _textAdvanceWaiter = textAdvanceWaiter;
        }

        public async ValueTask PlayScenario()
        {
            ScenarioData data = _scenarioRepo.FindById("test");
            using CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;

            foreach (IScenarioEvent e in data.Events)
            {
                await _handlerRepo.HandleAsync(e, token);
                if (e.RequirePlayerAdvance)
                {
                    await _textAdvanceWaiter.WaitNextAsync(token);
                }
            }
        }

        public ValueTask EmitAsync(IScenarioEvent scenarioEvent, CancellationToken ct)
        {
            return _handlerRepo.HandleAsync(scenarioEvent, ct);
        }

        private readonly ITextAdvanceWaiter _textAdvanceWaiter;
        private readonly ScenarioHandlerRepo _handlerRepo;
        private readonly IScenarioRepository _scenarioRepo;
    }
}
