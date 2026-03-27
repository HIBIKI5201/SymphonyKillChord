using System.Collections.Generic;
using KillChord.Runtime.View;
using R3;

namespace KillChord.Runtime.Adaptor
{
    public interface IMusicSyncViewModel
    {
        public ReadOnlyReactiveProperty<string> CueName { get; }
        ActionParams LastAction { get; }
        ActionParams Peek { get; }
        int Count { get; }
        ActionParams Dequeue();
        void Enqueue(ActionParams param);
        void UpdateMusicCue(string cueName);
    }
}