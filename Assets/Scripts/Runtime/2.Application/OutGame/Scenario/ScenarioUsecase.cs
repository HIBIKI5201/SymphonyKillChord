
using System.Collections.Generic;
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
            _handlerRepo = handlerRepo;
            _textAdvanceWaiter = textAdvanceWaiter;
        }

        public async ValueTask PlayScenario()
        {
            ScenarioData data = _scenarioRepo.FindById("test");
            using CancellationTokenSource cts = new CancellationTokenSource();
            foreach (IScenarioEvent e in data.Events)
            {
                // await _handlerRepo.HandleAsync(e, token);
                await ExecuteWithEmitsAsync(e, cts.Token);

                if (e.RequirePlayerAdvance)
                {
                    await _textAdvanceWaiter.WaitNextAsync(cts.Token);
                }
            }

        }
        private readonly ITextAdvanceWaiter _textAdvanceWaiter;
        private readonly ScenarioHandlerRepo _handlerRepo;
        private readonly IScenarioRepository _scenarioRepo;

        private async ValueTask ExecuteWithEmitsAsync(IScenarioEvent root, CancellationToken ct)
        {
            var queue = new Queue<IScenarioEvent>();
            queue.Enqueue(root);

            while (queue.Count > 0)
            {
                IScenarioEvent current = queue.Dequeue();
                ScenarioHandleResult result = await _handlerRepo.HandleAsync(current, ct);

                foreach (IScenarioEvent emitted in result.EmittedEvents)
                {
                    queue.Enqueue(emitted);
                }
            }
        }
    }
}
