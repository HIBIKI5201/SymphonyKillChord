using KillChord.Runtime.Domain.OutGame.Scenario;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Application.OutGame.Scenario
{
    /// <summary>
    /// シナリオ再生の進行と操作状態を管理するユースケース。
    /// </summary>
    public class ScenarioUsecase : IScenarioEventEmitter, IScenarioPlaybackControl, IScenarioPlaybackState, IScenarioPlaybackService
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
        public async ValueTask PlayScenario(string scenarioId)
        {
            if (string.IsNullOrWhiteSpace(scenarioId))
            {
                throw new ArgumentException("シナリオIDが設定されていません。", nameof(scenarioId));
            }

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

                    await _handlerRepo.HandleAsync(e, token);
                    bool isLastEvent = i == data.Events.Count - 1;
                    bool shouldWaitForAdvance = e.RequirePlayerAdvance
                        && (!isLastEvent || _settingsRepository.WaitForInputOnLastText);
                    if (shouldWaitForAdvance)
                    {
                        await WaitAdvanceAsync(token);
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

        /// <summary>
        /// 自動進行状態を切り替える。
        /// </summary>
        public void ToggleAutoAdvance()
        {
            IsAutoAdvance = !IsAutoAdvance;
        }

        /// <summary> IsFastForward を取得する。 </summary>
        public bool IsFastForward { get; private set; }
        /// <summary> IsPaused を取得する。 </summary>
        public bool IsPaused { get; private set; }
        /// <summary> IsAutoAdvance を取得する。 </summary>
        public bool IsAutoAdvance { get; private set; }

        private CancellationTokenSource _playCts;
        private readonly ITextAdvanceWaiter _textAdvanceWaiter;
        private readonly ScenarioHandlerRepo _handlerRepo;
        private readonly IScenarioRepository _scenarioRepo;
        private readonly IScenarioCompletionNotifier _completionNotifier;
        private readonly IScenarioSettingsRepository _settingsRepository;

        /// <summary>
        /// シナリオ再生の進行を待機する。
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async ValueTask WaitAdvanceAsync(CancellationToken ct)
        {
            if (!IsAutoAdvance)
            {
                // Autoではない場合は、クリックなどの手動送り入力が来るまで待機する。
                await _textAdvanceWaiter.WaitNextAsync(ct);
                return;
            }

            // Autoの場合は、設定された秒数だけ待ってから次のイベントへ進む。
            await WaitAutoAdvanceDelayAsync(ct);
        }

        /// <summary>
        ///     Auto時の次送り待機を行う。
        ///     Pause中は待機時間を進めない。
        /// </summary>
        /// <param name="ct">キャンセルトークン。</param>
        private async ValueTask WaitAutoAdvanceDelayAsync(CancellationToken ct)
        {
            // Autoで次へ進むまでの残り待機時間を設定から取得する。
            TimeSpan remainingDelay = _settingsRepository.AutoAdvanceDelay;

            while (remainingDelay > TimeSpan.Zero)
            {
                ct.ThrowIfCancellationRequested();

                if (IsPaused)
                {
                    // Pause中は残り時間を減らさず、短い間隔でPause解除を待つ。
                    await Task.Delay(_settingsRepository.PausePollInterval, ct);
                    continue;
                }

                // 残り時間より長く待たないように、今回待つ時間を決める。
                TimeSpan delay = remainingDelay < _settingsRepository.PausePollInterval
                    ? remainingDelay
                    : _settingsRepository.PausePollInterval;

                // 短い単位で待機することで、待機中のPause切り替えを反映しやすくする。
                await Task.Delay(delay, ct);

                // Pauseしていなかった分だけ、Auto待機の残り時間を減らす。
                remainingDelay -= delay;
            }

            if (!IsAutoAdvance)
            {
                // Auto待機中にAutoがOFFになった場合は、手動送り待機に戻す。
                await _textAdvanceWaiter.WaitNextAsync(ct);
            }
        }
    }
}
