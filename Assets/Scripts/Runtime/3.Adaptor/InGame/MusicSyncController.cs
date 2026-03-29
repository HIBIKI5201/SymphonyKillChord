using System;
using KillChord.Runtime.Utility;

namespace KillChord.Runtime.Adaptor
{
    public class MusicSyncController : IDisposable
    {
        private readonly IMusicSyncViewModel _musicSyncViewModel;

        public MusicSyncController(IMusicSyncViewModel musicSyncViewModel)
        {
            _musicSyncViewModel = musicSyncViewModel;
        }

        public void Dispose()
        {
        }
    }
}