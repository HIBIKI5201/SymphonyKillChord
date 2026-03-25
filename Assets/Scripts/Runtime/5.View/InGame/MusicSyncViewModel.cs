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

        public int GetNearestSignature(double seconds)
        {
            if (_bpm <= 0) return 4;

            double beatSeconds = 60d / _bpm;
            double barSeconds = beatSeconds * 4d;

            int nearestSignature = 1;
            double minDiff = double.MaxValue;

            for (int i = 1; i <= 8; i++)
            {
                double targetSeconds = barSeconds / i;
                double diff = Math.Abs(seconds - targetSeconds);

                if (diff < minDiff)
                {
                    minDiff = diff;
                    nearestSignature = i;
                }
            }

            return nearestSignature;
        }
    }
}
