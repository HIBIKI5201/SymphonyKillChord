using System;
using System.Collections.Generic;
using System.Threading;
using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Utility;

namespace KillChord.Runtime.View
{
    public class MusicSyncViewModel : IMusicSyncViewModel
    {
        public event Action OnUpdate;

        public double PlayTime { get; set; }
        public ActionParams LastAction => _actionList[^1];
        public ActionParams Peek => _actionList[0];
        public int Count => _actionList.Count;

        public int Bpm { get; set; }
        public int CurrentBeat { get; set; }
        public int NearestBeat { get; set; }
        public double BeatLength { get; set; }

        private List<ActionParams> _actionList = new();

        public void Update(double playTime)
        {
            PlayTime = playTime;
            OnUpdate?.Invoke();
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


        public Action<ExecuteRequestTiming, Action, CancellationToken> Register;

        public void RegisterAction(ExecuteRequestTiming timing, Action action, CancellationToken token)
        {
            Register?.Invoke(timing, action, token);
        }
    }
}