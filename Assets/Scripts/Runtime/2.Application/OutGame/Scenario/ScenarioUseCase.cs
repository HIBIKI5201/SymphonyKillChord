using KillChord.Runtime.Domain.OutGame.Scenario;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Application.OutGame.Scenario
{
    /// <summary>
    /// シナリオ再生の進行と操作状態を管理するユースケース。
    /// </summary>
    public class ScenarioUseCase : IScenarioEventEmitter, IScenarioPlaybackControl, IScenarioPlaybackState, IScenarioPlaybackService
    {
        /// <summary>
        /// シナリオ再生ユースケースの依存関係を受け取る。
        /// </summary>
        public ScenarioUseCase(
            IScenarioRepository repo,
            ScenarioHandlerRepository handlerRepo,
            ITextAdvanceWaiter textAdvanceWaiter,
            IScenarioCompletionNotifier completionNotifier,
            IScenarioAutoAdvanceNotifier autoAdvanceNotifier,
            IScenarioSettingsRepository settingsRepository)
        {
            _scenarioRepo = repo;
            _handlerRepo = handlerRepo;
            _textAdvanceWaiter = textAdvanceWaiter;
            _completionNotifier = completionNotifier;
            _autoAdvanceNotifier = autoAdvanceNotifier;
            _settingsRepository = settingsRepository;
            _autoAdvanceNotifier.NotifyAutoAdvanceChanged(IsAutoAdvance);
        }

        /// <summary>
        /// 設定されたシナリオを先頭から順に再生する。
        /// </summary>
        public async ValueTask PlayScenario(string scenarioId)
        {
            if (string.IsNullOrWhiteSpace(scenarioId))
            {
                throw new ArgumentException("シナリオIDが設定されていません。", nameof(scenarioId));
            }

            if (IsPlaying)
            {
                throw new InvalidOperationException("シナリオは既に再生中です。");
            }
            ResetPlaybackState();
            using CancellationTokenSource source = new CancellationTokenSource();
            _playCts = source;
            CancellationToken token = source.Token;
            bool skipped = false;
            try
            {
                // シナリオデータを読み込む。
                ScenarioDefinition data = await _scenarioRepo.FindByIdAsync(scenarioId, token);

                for (int i = 0; i < data.Events.Count; i++)
                {
                    IScenarioEvent e = data.Events[i];
                    token.ThrowIfCancellationRequested();

                    bool isLastEvent = i == data.Events.Count - 1;
                    bool shouldWaitForAdvance = e.RequirePlayerAdvance
                        && (!isLastEvent || _settingsRepository.WaitForInputOnLastText);
                    using CancellationTokenSource advanceSource =
                        CancellationTokenSource.CreateLinkedTokenSource(token);
                    // 表示完了と待機開始の隙間で入力を失わないよう、現在行の受付を先に開く。
                    Task advanceTask = shouldWaitForAdvance
                        ? _textAdvanceWaiter.WaitNextAsync(advanceSource.Token).AsTask()
                        : null;
                    try
                    {
                        await EmitAsync(e, token);
                        await WaitWhilePausedAsync(token);
                        if (shouldWaitForAdvance)
                        {
                            await WaitAdvanceAsync(advanceTask, token);
                        }
                    }
                    finally
                    {
                        // Autoと手動の先着一回で閉じ、余った入力や待機を次行へ渡さない。
                        advanceSource.Cancel();
                    }
                }
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                // スキップ要求時はシナリオを正常終了する。
                skipped = true;
            }
            finally
            {
                if (ReferenceEquals(_playCts, source))
                {
                    _playCts = null;
                    ResetPlaybackState();
                }
                await _completionNotifier.NotifyCompletedAsync(skipped, CancellationToken.None);
            }
        }

        /// <summary>
        /// 指定されたイベントを対応するハンドラへ引き渡す。
        /// </summary>
        public async ValueTask EmitAsync(IScenarioEvent scenarioEvent, CancellationToken ct)
        {
            await WaitWhilePausedAsync(ct);
            await _handlerRepo.HandleAsync(scenarioEvent, ct);
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
            NotifyAdvanceStateChanged();
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

        /// <summary>
        /// 自動進行状態を切り替える。
        /// </summary>
        public void ToggleAutoAdvance()
        {
            IsAutoAdvance = !IsAutoAdvance;
            _autoAdvanceVersion++;
            NotifyAdvanceStateChanged();
            _autoAdvanceNotifier.NotifyAutoAdvanceChanged(IsAutoAdvance);
        }

        /// <summary> シナリオが再生中かを示す。 </summary>
        public bool IsPlaying => _playCts != null;
        /// <summary> IsFastForward を取得する。 </summary>
        public bool IsFastForward { get; private set; }
        /// <summary> IsPaused を取得する。 </summary>
        public bool IsPaused { get; private set; }
        /// <summary> IsAutoAdvance を取得する。 </summary>
        public bool IsAutoAdvance { get; private set; }

        private static readonly TimeSpan MINIMUM_PAUSE_POLL_INTERVAL = TimeSpan.FromMilliseconds(10);

        private CancellationTokenSource _playCts;
        private int _autoAdvanceVersion;
        private TaskCompletionSource<bool> _advanceStateChanged =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly ITextAdvanceWaiter _textAdvanceWaiter;
        private readonly ScenarioHandlerRepository _handlerRepo;
        private readonly IScenarioRepository _scenarioRepo;
        private readonly IScenarioCompletionNotifier _completionNotifier;
        private readonly IScenarioAutoAdvanceNotifier _autoAdvanceNotifier;
        private readonly IScenarioSettingsRepository _settingsRepository;

        /// <summary>
        ///     一時停止が解除されるまで、キャンセルを受け付けながら待機する。
        /// </summary>
        private async ValueTask WaitWhilePausedAsync(CancellationToken ct)
        {
            while (IsPaused)
            {
                TimeSpan interval = _settingsRepository.PausePollInterval > TimeSpan.Zero
                    ? _settingsRepository.PausePollInterval
                    : MINIMUM_PAUSE_POLL_INTERVAL;
                await Task.Delay(interval, ct);
            }
            ct.ThrowIfCancellationRequested();
        }

        /// <summary>
        ///     現在行の手動送りとAuto待機を共有し、先に成立した一回だけ進行する。
        /// </summary>
        private async ValueTask WaitAdvanceAsync(Task advanceTask, CancellationToken ct)
        {
            TimeSpan remainingDelay = _settingsRepository.AutoAdvanceDelay;
            int autoAdvanceVersion = _autoAdvanceVersion;
            while (true)
            {
                ct.ThrowIfCancellationRequested();
                await WaitWhilePausedAsync(ct);
                if (advanceTask.IsCompleted)
                {
                    await advanceTask;
                    return;
                }

                // Autoを切り替えた場合は現在行の待ち時間を開始し直す。
                if (autoAdvanceVersion != _autoAdvanceVersion)
                {
                    autoAdvanceVersion = _autoAdvanceVersion;
                    remainingDelay = _settingsRepository.AutoAdvanceDelay;
                }
                if (IsAutoAdvance && remainingDelay <= TimeSpan.Zero)
                {
                    return;
                }

                Task stateChangedTask = _advanceStateChanged.Task;
                if (!IsAutoAdvance)
                {
                    await Task.WhenAny(advanceTask, stateChangedTask);
                    continue;
                }

                TimeSpan interval = _settingsRepository.PausePollInterval > TimeSpan.Zero
                    ? _settingsRepository.PausePollInterval
                    : MINIMUM_PAUSE_POLL_INTERVAL;
                TimeSpan delay = remainingDelay < interval ? remainingDelay : interval;
                using CancellationTokenSource delaySource = CancellationTokenSource.CreateLinkedTokenSource(ct);
                Task delayTask = Task.Delay(delay, delaySource.Token);
                Task completedTask = await Task.WhenAny(advanceTask, stateChangedTask, delayTask);
                delaySource.Cancel();
                if (ReferenceEquals(completedTask, delayTask)
                    && IsAutoAdvance && !IsPaused && autoAdvanceVersion == _autoAdvanceVersion)
                {
                    await delayTask;
                    remainingDelay -= delay;
                }
            }
        }

        /// <summary>
        ///     Auto・ポーズの切替を現在の次送り待機へ直ちに通知する。
        /// </summary>
        private void NotifyAdvanceStateChanged()
        {
            TaskCompletionSource<bool> previous = _advanceStateChanged;
            _advanceStateChanged = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            previous.TrySetResult(true);
        }

        /// <summary>
        ///     終了したシナリオの操作状態を次の再生へ持ち越さない。
        /// </summary>
        private void ResetPlaybackState()
        {
            IsFastForward = false;
            IsPaused = false;
            IsAutoAdvance = false;
            _autoAdvanceVersion++;
            NotifyAdvanceStateChanged();
            _autoAdvanceNotifier.NotifyAutoAdvanceChanged(false);
        }
    }
}
