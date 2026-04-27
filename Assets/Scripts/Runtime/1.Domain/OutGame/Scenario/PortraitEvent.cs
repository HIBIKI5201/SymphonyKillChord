using System;

namespace KillChord.Runtime.Domain
{
    public class PortraitEvent : IScenarioEvent
    {
        public const string DefaultSlotId = "Default";

        public PortraitEvent(string portraitId, string slotId = DefaultSlotId)
        {
            PortraitId = string.IsNullOrWhiteSpace(portraitId)
                ? throw new ArgumentException("portraitId is empty.", nameof(portraitId))
                : portraitId;
            SlotId = string.IsNullOrWhiteSpace(slotId) ? DefaultSlotId : slotId;
        }

        public string PortraitId { get; }
        public string SlotId { get; }
        public bool RequirePlayerAdvance => false;
    }
}
