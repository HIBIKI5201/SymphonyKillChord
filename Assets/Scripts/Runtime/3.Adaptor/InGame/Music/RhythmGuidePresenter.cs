using KillChord.Runtime.Adaptor.InGame.Target;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Domain.InGame.Music;
using System.Collections.Generic;
using KillChord.Runtime.Domain.InGame.Mission;

namespace KillChord.Runtime.Adaptor.InGame.Music
{
    /// <summary>
    ///     リズムガイドの表示用データを生成するプレゼンタークラス。
    /// </summary>
    public class RhythmGuidePresenter
    {
        /// <summary>
        ///     新しいプレゼンターを生成する。
        /// </summary>
        /// <param name="musicSyncService"> 音楽同期サービス。 </param>
        /// <param name="rhythmGuideUsecase"> リズムガイドユースケース。 </param>
        /// <param name="targetingSystem"> ターゲット選択システム。 </param>
        /// <param name="targetActionProvider"> チュートリアル中等のミッション指定アクション取得デリゲート。 </param>
        public RhythmGuidePresenter(IMusicSyncService musicSyncService, RhythmGuideUsecase rhythmGuideUsecase, TargetSystemController targetingSystem, System.Func<MissionActionKind?> targetActionProvider = null)
        {
            _musicSyncService = musicSyncService;
            _rhythmGuideUsecase = rhythmGuideUsecase;
            _targetingSystem = targetingSystem;
            _targetActionProvider = targetActionProvider;
        }

        /// <summary>
        ///     リズムガイドの表示用DTOを生成する。
        /// </summary>
        /// <returns> リズムガイドDTO。 </returns>
        public RhythmGuideDto CreateDto()
        {
            float barProgress = _musicSyncService.GetBarProgress();

            float indicatorNormalized = _rhythmGuideUsecase.CalculateIndicatorNormalized(barProgress);

            BeatType? currentBeatType = _rhythmGuideUsecase.CalculateCurrentBeatType(barProgress);

            int? currentBeatCount = currentBeatType.HasValue
                ? (int?)currentBeatType.Value : null;

            _zones.Clear();

            foreach (RhythmJudgmentRange range in _rhythmGuideUsecase.RhythmJudgmentDefinition.JudgmentRanges)
            {
                _zones.Add(new RhythmGuideZoneDto(
                    (int)range.BeatType,
                    range.StartNormalized,
                    range.EndNormalized
                ));
            }

            bool hasTarget = _targetingSystem.TryGetCurrentTargetEntity(out _);
            KillChord.Runtime.Domain.InGame.Mission.MissionActionKind? targetAction = _targetActionProvider?.Invoke();
            int? targetBeatCount = null;
            if (targetAction.HasValue)
            {
                targetBeatCount = targetAction.Value switch
                {
                    KillChord.Runtime.Domain.InGame.Mission.MissionActionKind.AttackOneBeat => 1,
                    KillChord.Runtime.Domain.InGame.Mission.MissionActionKind.AttackTwoBeat => 2,
                    KillChord.Runtime.Domain.InGame.Mission.MissionActionKind.AttackThreeBeat => 3,
                    KillChord.Runtime.Domain.InGame.Mission.MissionActionKind.AttackFourBeat => 4,
                    KillChord.Runtime.Domain.InGame.Mission.MissionActionKind.AttackSixBeat => 6,
                    KillChord.Runtime.Domain.InGame.Mission.MissionActionKind.AttackEightBeat => 8,
                    _ => null
                };
            }

            return new RhythmGuideDto(
                indicatorNormalized,
                currentBeatCount,
                _zones,
                hasTarget,
                targetBeatCount
            );
        }

        private readonly IMusicSyncService _musicSyncService;
        private readonly RhythmGuideUsecase _rhythmGuideUsecase;
        private readonly TargetSystemController _targetingSystem;
        private readonly System.Func<MissionActionKind?> _targetActionProvider;
        private readonly List<RhythmGuideZoneDto> _zones = new();
    }
}
