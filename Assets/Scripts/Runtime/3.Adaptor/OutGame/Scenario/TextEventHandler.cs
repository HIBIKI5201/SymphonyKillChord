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
        public TextEventHandler(ITextOutputPort textOutputPort, IScenarioEventEmitter eventEmitter)
        {
            _textOutputPort = textOutputPort;
            _eventEmitter = eventEmitter;
        }
        public Type EventType => typeof(TextEvent);

        public async ValueTask HandleAsync(TextEvent e, CancellationToken ct)
        {
            var fired = new HashSet<TextTimingTrigger>();

            for (int i = 1; i <= e.Text.Length; i++)
            {
                await _textOutputPort.ShowTextAsync($"{e.Speaker}: {e.Text[..i]}", ct);
                string visibleText = e.Text[..i];

                foreach (TextTimingTrigger trigger in e.Triggers)
                {
                    if (fired.Contains(trigger)) continue;
                    if (!TextTimingTrigger.ShouldFire(trigger, i, visibleText)) continue;

                    fired.Add(trigger);
                    await _eventEmitter.EmitAsync(trigger.FireEvent, ct);
                }

                await Task.Delay(200, ct);
            }
        }

        private readonly ITextOutputPort _textOutputPort;
        private readonly IScenarioEventEmitter _eventEmitter;

    }
}
