using System.Collections.Generic;
using KillChord.Runtime.Domain;

namespace KillChord.Runtime.InfraStructure
{
    internal abstract class EventDefinition
    {
        protected EventDefinition(int step) => Step = step;
        public int Step { get; }
        public abstract IScenarioEvent ToEvent();
    }

    internal sealed class PlainEventDefinition : EventDefinition
    {
        public PlainEventDefinition(int step, IScenarioEvent scenarioEvent) : base(step)
        {
            _scenarioEvent = scenarioEvent;
        }

        public override IScenarioEvent ToEvent() => _scenarioEvent;
        private readonly IScenarioEvent _scenarioEvent;
    }

    internal sealed class TextEventDefinition : EventDefinition
    {
        public TextEventDefinition(int step, string speaker, string text) : base(step)
        {
            Speaker = speaker;
            Text = text;
        }

        public string Speaker { get; }
        public string Text { get; }

        public void AddTrigger(TextTimingTrigger trigger) => _triggers.Add(trigger);

        public override IScenarioEvent ToEvent()
        {
            _cached ??= new TextEvent(Speaker, Text, _triggers.ToArray());
            return _cached;
        }

        private readonly List<TextTimingTrigger> _triggers = new();
        private IScenarioEvent _cached;
    }
}
