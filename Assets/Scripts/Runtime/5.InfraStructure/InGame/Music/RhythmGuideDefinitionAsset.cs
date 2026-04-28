using KillChord.Runtime.Domain.InGame.Music;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.InGame.Music
{
    [CreateAssetMenu(fileName = nameof(RhythmGuideDefinitionAsset), menuName = "KillChord/RhythmGuideDefinition")]
    public class RhythmGuideDefinitionAsset : ScriptableObject
    {
        public RhythmGuideDefinition ToDefinition()
        {
            List<RhythmGuideRange> guideRanges = new();

            foreach (RhythmGuideRangeData range in _rangeData)
            {
                guideRanges.Add(new RhythmGuideRange(range.BeatType, range.StartNormalized, range.EndNormalized));
            }

            return new RhythmGuideDefinition(guideRanges);
        }


        [SerializeField] private RhythmGuideRangeData[] _rangeData;

        [Serializable]
        private class RhythmGuideRangeData
        {
            public BeatType BeatType;

            [Range(0f, 1f)]
            public float StartNormalized;
            [Range(0f, 1f)]
            public float EndNormalized;
        }
    }
}
