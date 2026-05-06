using System;

namespace KillChord.Runtime.Domain
{
    public enum LayerTarget
    {
        Background,
        PortraitLeft,
        PortraitCenter,
        PortraitRight,
        Text,
        Canvas,
    }

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
