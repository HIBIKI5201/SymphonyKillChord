using System;
using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Domain;

namespace KillChord.Runtime.Application
{
    public class ScenarioUsecase : IScenarioEventEmitter, IScenarioPlaybackControl, IScenarioPlaybackState
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
            _playCts = source;
            CancellationToken token = source.Token;

            try
            {
                foreach (IScenarioEvent e in data.Events)
                {
                    token.ThrowIfCancellationRequested();

                    await _handlerRepo.HandleAsync(e, token);
                    if (e.RequirePlayerAdvance)
                    {
                        await _textAdvanceWaiter.WaitNextAsync(token);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Skip requested: end scenario gracefully.
            }
            finally
            {
                IsFastForward = false;
                IsPaused = false;
                if (ReferenceEquals(_playCts, source))
                {
                    _playCts = null;
                }
            }
        }

        public ValueTask EmitAsync(IScenarioEvent scenarioEvent, CancellationToken ct)
        {
            return _handlerRepo.HandleAsync(scenarioEvent, ct);
        }

        public void SetFastForward(bool enabled)
        {
            IsFastForward = enabled;
        }

        public void TogglePause()
        {
            IsPaused = !IsPaused;
        }

        public void RequestSkip()
        {
            _playCts?.Cancel();
        }

        public bool IsFastForward { get; private set; }
        public bool IsPaused { get; private set; }

        private CancellationTokenSource _playCts;
        private readonly ITextAdvanceWaiter _textAdvanceWaiter;
        private readonly ScenarioHandlerRepo _handlerRepo;
        private readonly IScenarioRepository _scenarioRepo;
    }
}
