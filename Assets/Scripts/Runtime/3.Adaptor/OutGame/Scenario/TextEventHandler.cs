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
        public TextEventHandler(ITextOutputPort textOutputPort, IScenarioEventEmitter eventEmitter, IScenarioPlaybackState playbackState)
        {
            _textOutputPort = textOutputPort;
            _eventEmitter = eventEmitter;
            _playbackState = playbackState;
        }
        public Type EventType => typeof(TextEvent);

        public async ValueTask HandleAsync(TextEvent e, CancellationToken ct)
        {
            var fired = new HashSet<TextTimingTrigger>();

            for (int i = 1; i <= e.Text.Length; i++)
            {
                while (_playbackState.IsPaused)
                {
                    await Task.Delay(50, ct);
                }

                await _textOutputPort.ShowTextAsync($"{e.Speaker}: {e.Text[..i]}", ct);
                string visibleText = e.Text[..i];

                foreach (TextTimingTrigger trigger in e.Triggers)
                {
                    if (fired.Contains(trigger)) continue;
                    if (!TextTimingTrigger.ShouldFire(trigger, i, visibleText)) continue;

                    fired.Add(trigger);
                    await _eventEmitter.EmitAsync(trigger.FireEvent, ct);
                }

                int delayMs = _playbackState.IsFastForward ? 20 : 200;
                await Task.Delay(delayMs, ct);
            }
        }

        private readonly ITextOutputPort _textOutputPort;
        private readonly IScenarioEventEmitter _eventEmitter;
        private readonly IScenarioPlaybackState _playbackState;

    }
}
