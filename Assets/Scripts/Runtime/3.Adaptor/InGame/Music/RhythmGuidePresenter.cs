using KillChord.Runtime.Adaptor.InGame.Camera.Target;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Domain.InGame.Music;
using System.Collections.Generic;

namespace KillChord.Runtime.Adaptor.InGame.Music
{
    public class RhythmGuidePresenter
    {
        public RhythmGuidePresenter(IMusicSyncService musicSyncService, RhythmGuideUsecase rhythmGuideUsecase, TargetSelectorController targetSelectorController)
        {
            _musicSyncService = musicSyncService;
            _rhythmGuideUsecase = rhythmGuideUsecase;
            _targetSelectorController = targetSelectorController;
        }

        public RhythmGuideDto CreateDto(float unscaledTime)
        {
            float barProgress = _musicSyncService.GetBarProgress(unscaledTime);

            float indicatorNormalized = _rhythmGuideUsecase.CalculateIndicatorNormalized(barProgress);

            BeatType? currentBeatType = _rhythmGuideUsecase.CalculateCurrentBeatType(barProgress);

            int? currentBeatCount = currentBeatType.HasValue
                ? (int)currentBeatType.Value : null;

            _zones.Clear();

            foreach (RhythmGuideRange range in _rhythmGuideUsecase.RhythmGuideDefinition.GuideRanges)
            {
                _zones.Add(new RhythmGuideZoneDto(
                    (int)range.BeatType,
                    range.StartNormalized,
                    range.EndNormalized
                ));
            }

            bool hasTarget = _targetSelectorController.TryGetCurrentTargetEntity(out _);

            return new RhythmGuideDto(
                indicatorNormalized,
                currentBeatCount,
                _zones,
                hasTarget
            );
        }

        private readonly IMusicSyncService _musicSyncService;
        private readonly RhythmGuideUsecase _rhythmGuideUsecase;
        private readonly TargetSelectorController _targetSelectorController;
        private readonly List<RhythmGuideZoneDto> _zones = new();
    }
}
