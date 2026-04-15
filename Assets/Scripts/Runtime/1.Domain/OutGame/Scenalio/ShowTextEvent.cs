using UnityEngine;

namespace KillChord.Runtime.Domain
{
    public class ShowTextEvent : IScenalioEvent
    {
        public ShowTextEvent(string message)
        {
            Message = message;
        }
        public string Message { get; }


    }
}
