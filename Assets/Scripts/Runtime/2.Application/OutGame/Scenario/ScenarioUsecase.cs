using System;
using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Domain.OutGame.Scenario;

namespace KillChord.Runtime.Application.OutGame.Scenario
{
    /// <summary>
    /// シナリオ再生の進行と操作状態を管理するユースケース。
    /// </summary>
    public class ScenarioUsecase : IScenarioEventEmitter, IScenarioPlaybackControl, IScenarioPlaybackState
    {
        /// <summary>
        /// シナリオ再生ユースケースの依存関係を受け取る。
        /// </summary>
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

        /// <summary>
        /// 設定されたシナリオを先頭から順に再生する。
        /// </summary>
        public async ValueTask PlayScenario()
        {
            using CancellationTokenSource source = new CancellationTokenSource();
            _playCts = source;
            CancellationToken token = source.Token;
            bool skipped = false;
            try
            {  
                ScenarioData data = await _scenarioRepo.FindByIdAsync(_settingsRepository.DefaultScenarioId, token); // シナリオデータを読み込む。
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
                // スキップ要求時はシナリオを正常終了する。
                skipped = true;
            }
            finally
            {
                await _completionNotifier.NotifyCompletedAsync(skipped, CancellationToken.None);
                if (ReferenceEquals(_playCts, source))
                {
                    _playCts = null;
                }
            }
        }

        /// <summary>
        /// 指定されたイベントを対応するハンドラへ引き渡す。
        /// </summary>
        public ValueTask EmitAsync(IScenarioEvent scenarioEvent, CancellationToken ct)
        {
            return _handlerRepo.HandleAsync(scenarioEvent, ct);
        }

        /// <summary>
        /// 早送り状態を切り替える。
        /// </summary>
        public void SetFastForward(bool enabled)
        {
            IsFastForward = enabled;
        }

        /// <summary>
        /// 一時停止状態を切り替える。
        /// </summary>
        public void TogglePause()
        {
            IsPaused = !IsPaused;
        }

        /// <summary>
        /// シナリオ再生のスキップを要求する。
        /// </summary>
        public void RequestSkip()
        {
            CancellationTokenSource cts = _playCts;
            if (cts == null) return;

            try
            {
                cts.Cancel();
            }
            catch (ObjectDisposedException)
            {
                // シナリオ再生終了後に CTS が破棄済みでも無視する。
            }
        }

        /// <summary> IsFastForward を取得する。 </summary>
        public bool IsFastForward { get; private set; }
        /// <summary> IsPaused を取得する。 </summary>
        public bool IsPaused { get; private set; }

        private CancellationTokenSource _playCts;
        private readonly ITextAdvanceWaiter _textAdvanceWaiter;
        private readonly ScenarioHandlerRepo _handlerRepo;
        private readonly IScenarioRepository _scenarioRepo;
        private readonly IScenarioCompletionNotifier _completionNotifier;
        private readonly IScenarioSettingsRepository _settingsRepository;
    }
}