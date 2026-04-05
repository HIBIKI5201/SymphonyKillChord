using System;
using System.Collections.Generic;
using KillChord.Runtime.Adaptor;

namespace KillChord.Runtime.View
{
    /// <summary>
    ///     音楽同期の状態を管理するViewModelクラス。
    /// </summary>
    public class MusicSyncViewModel : IMusicSyncViewModel
    {
        public event Action OnUpdate;

        public double PlayTime { get; set; }

        public int Bpm { get; set; }
        public int CurrentBeat { get; set; }
        public int NearestBeat { get; set; }
        public double AccurateBeat { get; set; }
        public double BeatLength { get; set; }

        public void Update(double playTime)
        {
            PlayTime = playTime;
            OnUpdate?.Invoke();
        }
    }
}