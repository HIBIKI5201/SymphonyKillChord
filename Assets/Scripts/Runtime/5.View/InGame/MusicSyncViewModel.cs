using System;
using System.Collections.Generic;
using KillChord.Runtime.Adaptor;
using KillChord.Runtime.View;

namespace KillChord.Runtime.View
{
    public class MusicSyncViewModel : IMusicSyncViewModel
    {
        public ActionParams LastAction => _actionList[^1];
        public ActionParams Peek => _actionList[0];
        public int Count => _actionList.Count;

        private List<ActionParams> _actionList = new();

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