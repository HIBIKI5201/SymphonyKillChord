using System;
using System.Collections.Generic;

namespace KillChord.Runtime.Domain.InGame.Music
{
    public class RhythmGuideDefinition
    {
        public RhythmGuideDefinition(IReadOnlyList<RhythmGuideRange> guideRanges)
        {
            if (guideRanges == null) throw new ArgumentNullException(nameof(guideRanges));
            _guideRanges = guideRanges;
        }

        public IReadOnlyList<RhythmGuideRange> GuideRanges => _guideRanges;

        private readonly IReadOnlyList<RhythmGuideRange> _guideRanges;
    }
}
