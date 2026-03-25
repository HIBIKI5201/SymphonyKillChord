using UnityEngine;

namespace KillChord.Runtime.View
{
    public readonly struct ActionParams
    {
        public readonly ActionType ActionType;
        public readonly int BeatType;
        public readonly float Timing;

        public ActionParams(ActionType actionType, int beatType)
        {
            BeatType = beatType;
            ActionType = actionType;
            Timing = Time.unscaledTime;
        }
    }
}