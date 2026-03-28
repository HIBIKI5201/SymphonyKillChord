using System;
using KillChord.Runtime.Utility;

namespace KillChord.Runtime.Adaptor
{
    public class MusicSyncController : IDisposable
    {
        private readonly IMusicSyncViewModel _musicSyncViewModel;
        
        private readonly PriorityQueue<ScheduledAction, double> _scheduledActions = new();

        public MusicSyncController(IMusicSyncViewModel musicSyncViewModel)
        {
            _musicSyncViewModel = musicSyncViewModel;
        }

        public void Dispose()
        {
        }
    }
}