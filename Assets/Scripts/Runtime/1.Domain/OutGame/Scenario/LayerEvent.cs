using System;

namespace KillChord.Runtime.Domain
{
    public class LayerEvent : IScenarioEvent
    {
        public LayerEvent(LayerTarget target, int order)
        {
            Target = target;
            Order = order;
        }

        public LayerTarget Target { get; }
        public int Order { get; }

        public bool RequirePlayerAdvance => false;
    }
}
