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
        public TextEventHandler(IOutPutPort outPutPort, IScenarioEventEmitter eventEmitter)
        {
            _outPort = outPutPort;
            _eventEmitter = eventEmitter;
        }
        public Type EventType => typeof(TextEvent);

        public async ValueTask HandleAsync(TextEvent e, CancellationToken ct)
        {
            var fired = new HashSet<TextTimingTrigger>();

            for (int i = 1; i <= e.Text.Length; i++)
            {
                await _outPort.ShowTextAsync($"{e.Speaker}: {e.Text[..i]}", ct);
                string visibleText = e.Text[..i];

                foreach (TextTimingTrigger trigger in e.Triggers)
                {
                    if (fired.Contains(trigger)) continue;
                    if (!ShouldFire(trigger, i, visibleText)) continue;

                    fired.Add(trigger);
                    await _eventEmitter.EmitAsync(trigger.FireEvent, ct);
                }

                await Task.Delay(200, ct);
            }
        }

        private readonly IOutPutPort _outPort;
        private readonly IScenarioEventEmitter _eventEmitter;

        private static bool ShouldFire(TextTimingTrigger trigger, int visibleCharCount, string visibleText)
        {
            return trigger.Kind switch
            {
                TextTriggerKind.CharIndex => trigger.CharIndex == visibleCharCount,
                TextTriggerKind.Keyword => !string.IsNullOrEmpty(trigger.Keyword) &&
                                           visibleText.Contains(trigger.Keyword, StringComparison.Ordinal),
                _ => false
            };
        }
    }
}
