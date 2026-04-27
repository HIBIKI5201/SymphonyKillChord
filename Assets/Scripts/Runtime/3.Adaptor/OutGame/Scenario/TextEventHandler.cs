using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KillChord.Runtime.Application;
using KillChord.Runtime.Domain;

namespace KillChord.Runtime.Adaptor
{
    public class TextEventHandler : IScenarioEventHandler<TextEvent>
    {
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
        public async ValueTask HandleAsync(TextEvent e, CancellationToken ct)
        {
            var fired = new HashSet<TextTimingTrigger>();
            await TryFireTriggersAsync(e.Triggers, fired, 0, string.Empty, ct);

            for (int i = 1; i <= e.Text.Length; i++)
            {
                while (_playbackState.IsPaused)
                {
                    TimeSpan pauseDelay = _settingsRepository.PausePollInterval < TimeSpan.FromMilliseconds(10)
                        ? TimeSpan.FromMilliseconds(10)
                        : _settingsRepository.PausePollInterval;
                    await Task.Delay(pauseDelay, ct);
                }

                await _textOutputPort.ShowTextAsync($"{e.Speaker}: {e.Text[..i]}", ct);
                string visibleText = e.Text[..i];

                await TryFireTriggersAsync(e.Triggers, fired, i, visibleText, ct);

                TimeSpan delay = _playbackState.IsFastForward
                    ? _settingsRepository.FastForwardTextCharInterval
                    : _settingsRepository.NormalTextCharInterval;
                if (delay > TimeSpan.Zero)
                {
                    await Task.Delay(delay, ct);
                }
            }
        }

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

        private readonly ITextOutputPort _textOutputPort;
        private readonly IScenarioEventEmitter _eventEmitter;
        private readonly IScenarioPlaybackState _playbackState;
        private readonly IScenarioSettingsRepository _settingsRepository;

    }
}
