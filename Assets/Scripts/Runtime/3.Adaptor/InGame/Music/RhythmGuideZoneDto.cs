using System.Drawing;

namespace KillChord.Runtime.Adaptor.InGame.Music
{
    public readonly struct RhythmGuideZoneDto
    {
        public RhythmGuideZoneDto(int beatCount, float startNormalized, float endNormalized)
        {
            BeatCount = beatCount;
            StartNormalized = startNormalized;
            EndNormalized = endNormalized;
        }

        public int BeatCount { get; }
        public float StartNormalized { get; }
        public float EndNormalized { get; }
    }
}
