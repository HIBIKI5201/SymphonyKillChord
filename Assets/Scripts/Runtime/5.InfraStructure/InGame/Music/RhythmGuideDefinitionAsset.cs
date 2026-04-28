using KillChord.Runtime.Domain.InGame.Music;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace KillChord.Runtime.InfraStructure.InGame.Music
{
    [CreateAssetMenu(fileName = nameof(RhythmGuideDefinitionAsset), menuName = "KillChord/RhythmGuideDefinition")]
    public class RhythmGuideDefinitionAsset : ScriptableObject
    {
        public RhythmGuideDefinition ToDefinition()
        {
            if (_rangeData == null || _rangeData.Length == 0)
            {
                return new RhythmGuideDefinition(Array.Empty<RhythmGuideRange>());
            }

            List<RhythmGuideRange> guideRanges = new(_rangeData.Length);

            for (int i = 0; i < _rangeData.Length; i++)
            {
                RhythmGuideRangeData range = _rangeData[i];
                if (range == null)
                {
                    throw new InvalidOperationException($"_rangeData[{i}] is null.");
                }
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
