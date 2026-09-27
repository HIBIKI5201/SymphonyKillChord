using KillChord.Runtime.Adaptor.InGame.Mission;
using KillChord.Runtime.Adaptor.InGame.StageSelect;
using KillChord.Runtime.Adaptor.InGame.Target;
using KillChord.Runtime.Application.InGame.Mission;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Domain.InGame.Music;
using System;
using System.Collections.Generic;

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
        /// <param name="missionRuntimeServiceProvider">
        ///     現在有効なミッション実行サービスを取得するデリゲート。
        ///     ミッション遷移でインスタンスが差し替わるため、Presenter側でインスタンスをキャッシュせず毎回取得する。
        ///     未使用の場合はnull。
        /// </param>
        /// <param name="selectedBattleStageState"> チュートリアル判定に使用する選択中ステージ状態。未使用の場合はnull。 </param>
        public RhythmGuidePresenter(
            IMusicSyncService musicSyncService,
            RhythmGuideUsecase rhythmGuideUsecase,
            TargetSystemController targetingSystem,
            Func<MissionRuntimeService> missionRuntimeServiceProvider = null,
            SelectedBattleStageState selectedBattleStageState = null)
        {
            _musicSyncService = musicSyncService;
            _rhythmGuideUsecase = rhythmGuideUsecase;
            _targetingSystem = targetingSystem;
            _missionRuntimeServiceProvider = missionRuntimeServiceProvider;
            _selectedBattleStageState = selectedBattleStageState;
        }

        /// <summary>
        ///     リズムガイドの表示用DTOを生成する。
        /// </summary>
        /// <returns> リズムガイドDTO。 </returns>
        public RhythmGuideDto CreateDto()
        {
            // インジケーターはジャスト位置を通過させるため1小節を超えた進捗を使用する。
            float indicatorBarProgress = _musicSyncService.GetBarProgressUnclamped();

            float indicatorNormalized = _rhythmGuideUsecase.CalculateIndicatorNormalized(indicatorBarProgress);

            BeatType currentBeatType = _musicSyncService.GetCurrentBeatType(out bool isJustHit);
            int currentBeatCount = (int)currentBeatType;

            RefreshZonesIfDefinitionChanged();

            bool hasTarget = _targetingSystem.TryGetCurrentTargetEntity(out _);
            int? targetBeatCount = GetTutorialTargetBeatCount();

            return new RhythmGuideDto(
                indicatorNormalized,
                currentBeatCount,
                _zones,
                hasTarget,
                isJustHit,
                _musicSyncService.RhythmJudgmentDefinition.TimeoutBarCount,
                targetBeatCount
            );
        }

        /// <summary>
        ///     判定定義が変わった時だけ、表示用の判定ゾーン一覧を作り直す。
        ///     判定定義はプレイ中に変わらないため、通常は初回の1度だけ作る。
        /// </summary>
        private void RefreshZonesIfDefinitionChanged()
        {
            RhythmJudgmentDefinition definition = _musicSyncService.RhythmJudgmentDefinition;
            if (ReferenceEquals(definition, _zonesSourceDefinition))
            {
                return;
            }

            _zonesSourceDefinition = definition;
            _zones.Clear();
            if (definition == null)
            {
                return;
            }

            // インターフェース越しのforeachは列挙子がボクシングされるため、インデックスで回す。
            IReadOnlyList<RhythmJudgmentRange> ranges = definition.JudgmentRanges;
            for (int i = 0; i < ranges.Count; i++)
            {
                RhythmJudgmentRange range = ranges[i];
                _zones.Add(new RhythmGuideZoneDto(
                    (int)range.BeatType,
                    range.StartNormalized,
                    range.EndNormalized,
                    range.JustStartNormalized,
                    range.JustEndNormalized
                ));
            }
        }

        /// <summary>
        ///     チュートリアル中に指定されているミッションアクションのBeatCountを取得する。
        /// </summary>
        /// <returns> 対象が存在しない、またはチュートリアル中でない場合はnull。 </returns>
        private int? GetTutorialTargetBeatCount()
        {
            // ミッション遷移でインスタンスが差し替わるため、都度最新のサービスを取得する。
            return TutorialAttackTargetQuery.GetTargetBeatCount(
                _selectedBattleStageState, _missionRuntimeServiceProvider?.Invoke());
        }

        private readonly IMusicSyncService _musicSyncService;
        private readonly RhythmGuideUsecase _rhythmGuideUsecase;
        private readonly TargetSystemController _targetingSystem;
        private readonly Func<MissionRuntimeService> _missionRuntimeServiceProvider;
        private readonly SelectedBattleStageState _selectedBattleStageState;
        private readonly List<RhythmGuideZoneDto> _zones = new();
        private RhythmJudgmentDefinition _zonesSourceDefinition;
    }
}
