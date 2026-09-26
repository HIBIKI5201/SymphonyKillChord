using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Application.OutGame.Scenario;
using KillChord.Runtime.Domain.OutGame.Scenario;

namespace KillChord.Runtime.Adaptor.OutGame.Scenario
{
    /// <summary>
    /// テキストイベントを逐次表示しトリガー発火も管理する。
    /// </summary>
    public class TextEventHandler : IScenarioEventHandler<TextEvent>
    {
        /// <summary>
        /// テキスト表示とトリガー処理に必要な依存関係を受け取る。
        /// </summary>
        public TextEventHandler(
            ITextOutputPort textOutputPort,
            IScenarioEventEmitter eventEmitter,
            IScenarioPlaybackState playbackState,
            IScenarioSettingsRepository settingsRepository)
        {
            _textOutputPort = textOutputPort;
            _eventEmitter = eventEmitter;
            _playbackState = playbackState;
            _settingsRepository = settingsRepository;
        }

        /// <summary>
        /// 進行中の文字送りを完了し、現在のテキストを全文表示する。
        /// </summary>
        /// <returns> 文字送り中の完了要求を受理した場合はtrue。 </returns>
        public bool TryCompleteCurrentText()
        {
            lock (_textRevealSync)
            {
                if (_textRevealCompletionSource == null)
                {
                    return false;
                }

                _textRevealCompletionSource.TrySetResult(true);
                return true;
            }
        }

        /// <summary>
        /// 受け取ったイベントを現在の出力先へ反映する。
        /// </summary>
        public async ValueTask HandleAsync(TextEvent e, CancellationToken ct)
        {
            TaskCompletionSource<bool> completionSource = BeginTextReveal();
            var fired = new HashSet<TextTimingTrigger>();
            try
            {
                // 話者を表示し、0文字目のトリガーを発火する。
                await _textOutputPort.ShowTextAsync(e.Speaker, string.Empty, ct);
                await TryFireTriggersAsync(e.Triggers, fired, 0, string.Empty, ct);

                // 1文字ずつ本文を表示する。送りの要求が来たら残りを一度に表示して終える。
                for (int i = 1; i <= e.Text.Length; i++)
                {
                    if (completionSource.Task.IsCompleted)
                    {
                        await CompleteTextAsync(e, fired, i - 1, ct);
                        break;
                    }

                    // 一時停止中は、送りの要求を待ちながら再開まで待つ。
                    while (_playbackState.IsPaused)
                    {
                        TimeSpan pauseDelay = _settingsRepository.PausePollInterval < TimeSpan.FromMilliseconds(10)
                            ? TimeSpan.FromMilliseconds(10)
                            : _settingsRepository.PausePollInterval;
                        if (await WaitForCompletionAsync(completionSource.Task, pauseDelay, ct))
                        {
                            await CompleteTextAsync(e, fired, i - 1, ct);
                            return;
                        }
                    }

                    // 表示した文字数に応じたトリガーを発火する。
                    await _textOutputPort.ShowTextAsync(e.Speaker, e.Text[..i], ct);
                    string visibleText = e.Text[..i];

                    await TryFireTriggersAsync(e.Triggers, fired, i, visibleText, ct);

                    if (i >= e.Text.Length)
                    {
                        continue;
                    }

                    // 早送り中は短い間隔で次の文字へ進む。
                    TimeSpan delay = _playbackState.IsFastForward
                        ? _settingsRepository.FastForwardTextCharInterval
                        : _settingsRepository.NormalTextCharInterval;
                    if (delay > TimeSpan.Zero
                        && await WaitForCompletionAsync(completionSource.Task, delay, ct))
                    {
                        await CompleteTextAsync(e, fired, i, ct);
                        break;
                    }
                }
            }
            finally
            {
                EndTextReveal(completionSource);
            }
        }

        /// <summary>
        /// 表示済み文字数に応じて発火条件を満たしたトリガーを実行する。
        /// </summary>
        private async ValueTask TryFireTriggersAsync(
            IReadOnlyList<TextTimingTrigger> triggers,
            HashSet<TextTimingTrigger> fired,
            int visibleCharCount,
            string visibleText,
            CancellationToken ct)
        {
            foreach (TextTimingTrigger trigger in triggers)
            {
                if (fired.Contains(trigger)) continue;
                if (!TextTimingTrigger.ShouldFire(trigger, visibleCharCount, visibleText)) continue;

                fired.Add(trigger);
                await _eventEmitter.EmitAsync(trigger.FireEvent, ct);
            }
        }

        /// <summary>
        /// 現在のテキストを全文表示し、未発火のタイミングトリガーを処理する。
        /// </summary>
        private async ValueTask CompleteTextAsync(
            TextEvent e,
            HashSet<TextTimingTrigger> fired,
            int visibleCharCount,
            CancellationToken ct)
        {
            await _textOutputPort.ShowTextAsync(e.Speaker, e.Text, ct);
            for (int i = visibleCharCount + 1; i <= e.Text.Length; i++)
            {
                await TryFireTriggersAsync(e.Triggers, fired, i, e.Text[..i], ct);
            }
        }

        /// <summary>
        /// 文字送りの完了要求を受け付ける状態を開始する。
        /// </summary>
        private TaskCompletionSource<bool> BeginTextReveal()
        {
            lock (_textRevealSync)
            {
                if (_textRevealCompletionSource != null)
                {
                    throw new InvalidOperationException("文字送りは既に開始されています。");
                }

                _textRevealCompletionSource = new TaskCompletionSource<bool>(
                    TaskCreationOptions.RunContinuationsAsynchronously);
                return _textRevealCompletionSource;
            }
        }

        /// <summary>
        /// 文字送りの完了要求受付を終了する。
        /// </summary>
        private void EndTextReveal(TaskCompletionSource<bool> completionSource)
        {
            lock (_textRevealSync)
            {
                if (ReferenceEquals(_textRevealCompletionSource, completionSource))
                {
                    _textRevealCompletionSource = null;
                }
            }
        }

        /// <summary>
        /// 指定時間の経過または文字送り完了要求を待機する。
        /// </summary>
        /// <returns> 文字送り完了要求を受け取った場合はtrue。 </returns>
        private static async ValueTask<bool> WaitForCompletionAsync(
            Task completionTask,
            TimeSpan delay,
            CancellationToken ct)
        {
            Task delayTask = Task.Delay(delay, ct);
            Task completedTask = await Task.WhenAny(delayTask, completionTask);
            if (ReferenceEquals(completedTask, completionTask))
            {
                ct.ThrowIfCancellationRequested();
                return true;
            }

            await delayTask;
            return false;
        }

        private readonly ITextOutputPort _textOutputPort;
        private readonly IScenarioEventEmitter _eventEmitter;
        private readonly IScenarioPlaybackState _playbackState;
        private readonly IScenarioSettingsRepository _settingsRepository;
        private readonly object _textRevealSync = new();
        private TaskCompletionSource<bool> _textRevealCompletionSource;
    }
}
