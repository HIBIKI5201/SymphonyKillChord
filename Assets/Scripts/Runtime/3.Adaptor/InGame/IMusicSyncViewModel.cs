using System;
using System.Threading;
using KillChord.Runtime.View;

namespace KillChord.Runtime.Adaptor
{
    public interface IMusicSyncViewModel
    {
        event Action OnUpdate;
        double PlayTime { get; }
        ActionParams LastAction { get; }
        ActionParams Peek { get; }
        int Count { get; }

        public int Bpm { get; }

        /// <summary> 最も近く過ぎた拍を取得する </summary>
        int CurrentBeat { get; }

        /// <summary> 最も近い拍を取得する </summary>
        public int NearestBeat { get; }

        /// <summary> 一拍の長さ </summary>
        public double BeatLength { get; }

        ActionParams Dequeue();
        void Enqueue(ActionParams param);
        void RegisterAction(ExecuteRequestTiming timing, Action action, CancellationToken token);
    }
}