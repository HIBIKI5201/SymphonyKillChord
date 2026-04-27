using System;
using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Domain;

namespace KillChord.Runtime.Application
{
    public class ScenarioUsecase : IScenarioEventEmitter, IScenarioPlaybackControl, IScenarioPlaybackState
    {
        public ScenarioUsecase(
            IScenarioRepository repo,
            ScenarioHandlerRepo handlerRepo,
            ITextAdvanceWaiter textAdvanceWaiter,
            IScenarioCompletionNotifier completionNotifier,
            IScenarioSettingsRepository settingsRepository)
        {
            _scenarioRepo = repo;
            _handlerRepo = handlerRepo;
            _textAdvanceWaiter = textAdvanceWaiter;
            _completionNotifier = completionNotifier;
            _settingsRepository = settingsRepository;
        }

        public async ValueTask PlayScenario()
        {
            using CancellationTokenSource source = new CancellationTokenSource();
            _playCts = source;
            CancellationToken token = source.Token;
            bool skipped = false;
            ScenarioData data = await _scenarioRepo.FindByIdAsync(_settingsRepository.DefaultScenarioId, token);

            try
            {
                for (int i = 0; i < data.Events.Count; i++)
                {
                    IScenarioEvent e = data.Events[i];
                    token.ThrowIfCancellationRequested();

                    await _handlerRepo.HandleAsync(e, token);
                    bool isLastEvent = i == data.Events.Count - 1;
                    bool shouldWaitForAdvance = e.RequirePlayerAdvance
                        && (!isLastEvent || _settingsRepository.WaitForInputOnLastText);
                    if (shouldWaitForAdvance)
                    {
                        await _textAdvanceWaiter.WaitNextAsync(token);
                    }
                }
            }
            catch (OperationCanceledException ex) when (ex.CancellationToken == token)
            {
                // Skip requested: end scenario gracefully.
                skipped = true;
            }
            finally
            {
                await _completionNotifier.NotifyCompletedAsync(skipped, CancellationToken.None);
                _playCts = null;
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
        private readonly IScenarioCompletionNotifier _completionNotifier;
        private readonly IScenarioSettingsRepository _settingsRepository;
    }
}
