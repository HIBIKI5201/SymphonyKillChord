using System;

namespace KillChord.Runtime.Domain
{
    public enum PortraitSlot
    {
        Left,
        Center,
        Right,
    }

    public class PortraitEvent : IScenarioEvent
    {
        public PortraitEvent(
            PortraitSlot slot,
            string portraitId,
            float positionX,
            float positionY,
            float scale,
            bool visible)
        {
            if (string.IsNullOrWhiteSpace(portraitId))
            {
                throw new ArgumentException("portraitId is empty.", nameof(portraitId));
            }

            Slot = slot;
            PortraitId = portraitId;
            PositionX = positionX;
            PositionY = positionY;
            Scale = scale <= 0f ? 1f : scale;
            Visible = visible;
        }

        public PortraitSlot Slot { get; }
        public string PortraitId { get; }
        public float PositionX { get; }
        public float PositionY { get; }
        public float Scale { get; }
        public bool Visible { get; }

        public bool RequirePlayerAdvance => false;
    }
}
