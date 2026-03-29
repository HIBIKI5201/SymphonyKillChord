using System;
using KillChord.Runtime.Application;
using KillChord.Runtime.Utility;

namespace KillChord.Runtime.Adaptor
{
    public class MusicSyncController : IDisposable
    {
        private readonly IMusicSyncViewModel _musicSyncViewModel;
        private readonly IMusicSyncService _musicSyncService;

        public MusicSyncController(IMusicSyncViewModel musicSyncViewModel, IMusicSyncService musicSyncService)
        {
            _musicSyncViewModel = musicSyncViewModel;
            _musicSyncService = musicSyncService;

            _musicSyncViewModel.OnUpdate += Tick;
        }

        public void Tick()
        {
            _musicSyncService.Update(_musicSyncViewModel.PlayTime);
        }

        public void Dispose()
        {
            _musicSyncViewModel.OnUpdate -= Tick;
        }
    }
}