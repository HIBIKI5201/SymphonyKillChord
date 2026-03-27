using System;
using System.Threading;
using KillChord.Runtime.View;

namespace KillChord.Runtime.Adaptor
{
    public interface IMusicSyncViewModel
    {
        ActionParams LastAction { get; }
        ActionParams Peek { get; }
        int Count { get; }

        /// <summary> 最も近く過ぎた拍を取得する </summary>
        int CurrentBeat { get; }

        /// <summary> 最も近い拍を取得する </summary>
        public int NearestBeat { get; }
        ActionParams Dequeue();
        void Enqueue(ActionParams param);
        void RegisterAction(ExecuteRequestTiming timing, Action action, CancellationToken token);
    }
}