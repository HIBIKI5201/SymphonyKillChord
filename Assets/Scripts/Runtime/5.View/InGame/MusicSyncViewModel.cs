using System;
using System.Collections.Generic;
using KillChord.Runtime.Adaptor;
using KillChord.Runtime.View;
using R3;

namespace KillChord.Runtime.View
{
    public class MusicSyncViewModel : IMusicSyncViewModel
    {
        public ReadOnlyReactiveProperty<string> CueName => _cueName;

        private ReactiveProperty<string> _cueName = new(string.Empty);
        public ActionParams LastAction => _actionList[^1];
        public ActionParams Peek => _actionList[0];
        public int Count => _actionList.Count;

        private List<ActionParams> _actionList = new();

        public void UpdateMusicCue(string cueName)
        {
            _cueName.Value = cueName;
        }

        public ActionParams Dequeue()
        {
            var returnParam = _actionList[0];
            _actionList.RemoveAt(0);
            return returnParam;
        }

        public void Enqueue(ActionParams param)
        {
            _actionList.Add(param);
        }
    }
}