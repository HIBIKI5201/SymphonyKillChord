using KillChord.Runtime.Adaptor.InGame.StageSelect;
using KillChord.Runtime.Adaptor.InGame.Target;
using KillChord.Runtime.Application.InGame.Mission;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Domain.InGame.Mission.ClearCondition;
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
            float barProgress = _musicSyncService.GetBarProgress();

            // インジケーターはジャスト位置を通過させるため1小節を超えた進捗を使用する。
            // 拍種の解決は0〜1の判定範囲を前提とするため、クランプ済みの進捗をそのまま使う。
            float indicatorBarProgress = _musicSyncService.GetBarProgressUnclamped();

            float indicatorNormalized = _rhythmGuideUsecase.CalculateIndicatorNormalized(indicatorBarProgress);

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
            int? targetBeatCount = GetTutorialTargetBeatCount();

            return new RhythmGuideDto(
                indicatorNormalized,
                currentBeatCount,
                _zones,
                hasTarget,
                targetBeatCount
            );
        }

        /// <summary>
        ///     チュートリアル中に指定されているミッションアクションのBeatCountを取得する。
        /// </summary>
        /// <returns> 対象が存在しない、またはチュートリアル中でない場合はnull。 </returns>
        private int? GetTutorialTargetBeatCount()
        {
            if (_selectedBattleStageState == null
                || !_selectedBattleStageState.HasSelectedBattleStage
                || !_selectedBattleStageState.CurrentStageDefinition.IsTutorial)
            {
                return null;
            }

            // ミッション遷移でインスタンスが差し替わるため、都度最新のサービスを取得する。
            MissionRuntimeService missionRuntimeService = _missionRuntimeServiceProvider?.Invoke();

            if (missionRuntimeService?.MissionDefinition?.ClearCondition is not ObjectiveSequenceClearCondition sequence)
            {
                return null;
            }

            int currentStepIndex = missionRuntimeService.MissionProgress.ObjectiveStepIndex;
            var currentStep = sequence.GetStep(currentStepIndex);

            if (currentStep?.Condition is not ActionRepeatCountClearCondition actionCondition)
            {
                return null;
            }

            int? result = actionCondition.TargetBeatType.HasValue
                ? (int)actionCondition.TargetBeatType.Value
                : null;

            return result;
        }

        private readonly IMusicSyncService _musicSyncService;
        private readonly RhythmGuideUsecase _rhythmGuideUsecase;
        private readonly TargetSystemController _targetingSystem;
        private readonly Func<MissionRuntimeService> _missionRuntimeServiceProvider;
        private readonly SelectedBattleStageState _selectedBattleStageState;
        private readonly List<RhythmGuideZoneDto> _zones = new();
    }
}