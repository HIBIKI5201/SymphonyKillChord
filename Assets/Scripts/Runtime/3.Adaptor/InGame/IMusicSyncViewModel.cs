using System;

namespace KillChord.Runtime.Adaptor
{
    public interface IMusicSyncViewModel
    {
        event Action OnUpdate;
        double PlayTime { get; }

        public int Bpm { get; }

        /// <summary> 最も近く過ぎた拍を取得する </summary>
        int CurrentBeat { get; }

        /// <summary> 最も近い拍を取得する </summary>
        public int NearestBeat { get; }

        /// <summary> double精度の拍を取得する </summary>
        public double AccurateBeat { get; }

        /// <summary> 一拍の長さ </summary>
        public double BeatLength { get; }
    }
}