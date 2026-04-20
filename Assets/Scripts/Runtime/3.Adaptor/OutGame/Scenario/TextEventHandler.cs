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
        public TextEventHandler(IOutPutPort outPutPort)
        {
            _outPort = outPutPort;
        }
        public Type EventType => typeof(TextEvent);

        public async ValueTask<ScenarioHandleResult> HandleAsync(TextEvent e, CancellationToken ct)
        {
            var emitted = new List<IScenarioEvent>();
            var fired = new HashSet<TextTimingTrigger>();

            for (int i = 1; i <= e.Text.Length; i++)
            {
                await _outPort.ShowTextAsync($"{e.Speaker}: {e.Text[..i]}", ct);

                foreach (TextTimingTrigger trigger in e.Triggers)
                {
                    if (fired.Contains(trigger)) continue;
                    if (trigger.CharIndex != i) continue;

                    fired.Add(trigger);
                    emitted.Add(trigger.FireEvent);
                }

                await Task.Delay(30, ct);
            }

            return new ScenarioHandleResult(emitted);
        }

        private readonly IOutPutPort _outPort;

    }
}
