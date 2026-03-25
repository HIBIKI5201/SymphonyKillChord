using System;
using KillChord.Runtime.Adaptor;

namespace KillChord.Runtime.View
{
    public class MusicSyncViewModel : IMusicSyncViewModel
    {
        private int _bpm;

        public void SetBpm(int bpm)
        {
            _bpm = bpm;
        }

        public void RegisterActionType(ActionType actionType)
        {
            
        }
    }
}
