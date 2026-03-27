using System.Collections.Generic;
using KillChord.Runtime.View;

namespace KillChord.Runtime.Adaptor
{
    public interface IMusicSyncViewModel
    {
        ActionParams LastAction { get; }
        ActionParams Peek { get; }
        int Count { get; }
        ActionParams Dequeue();
        void Enqueue(ActionParams param);
    }
}