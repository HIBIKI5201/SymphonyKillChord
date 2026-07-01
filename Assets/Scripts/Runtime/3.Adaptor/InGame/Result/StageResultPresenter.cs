using KillChord.Runtime.Adaptor.InGame.StageSelect;
using KillChord.Runtime.Application.InGame.Mission;
using KillChord.Runtime.Domain.InGame.Mission;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Result
{
    /// <summary>
    ///    ステージ結果画面の表示を行うプレゼンター。
    /// </summary>
    public class StageResultPresenter
    {
        private const string MAIN_MISSION_CLEAR_TEXT = "達成";
        private const string MAIN_MISSION_FAIL_TEXT = "失敗";
        private const string UNIMPLEMENTED_VALUE_TEXT = "未実装";

        private const string DEFAULT_DEFEAT_TIP_TEXT = "敵の攻撃をよくみて、回避後の隙に攻撃しましょう。";
        private const int SECOND_PER_MINUTE = 60;

        /// <summary>
        ///    ステージ結果画面の表示を行うプレゼンターを初期化する。
        /// </summary>
        /// <param name="missionRuntimeService"> ミッションのランタイムサービス。 </param>
        /// <param name="selectedBattleStageState">選択されたバトルステージの状態。 </param>
        /// <param name="viewModel">ステージ結果のViewModel。 </param>
        /// <exception cref="ArgumentNullException"></exception>
        public StageResultPresenter(
            MissionRuntimeService missionRuntimeService,
            SelectedBattleStageState selectedBattleStageState,
            IStageResultViewModel viewModel
            )
        {
            _missionRuntimeService = missionRuntimeService
                ?? throw new ArgumentNullException(
                    nameof(missionRuntimeService));

            _selectedBattleStageState = selectedBattleStageState
                ?? throw new ArgumentNullException(
                    nameof(selectedBattleStageState));

            _stageResultViewModel = viewModel
                ?? throw new ArgumentNullException(
                    nameof(viewModel));
        }

        /// <summary>
        ///   ステージクリアの結果を表示する。
        /// </summary>
        public void PresentVictory()
        {
            MissionEvaluationResult evaluationResult =
                _missionRuntimeService.BuildEvaluationResult();

            StageResultMissionItemDTO[] subMissionItems = BuildSubMissionItems(evaluationResult);

            StageResultDTO dto = new(
                StageResultType.Victory,
                _selectedBattleStageState.StageName,
                _missionRuntimeService.MissionDefinition.MainMissionText,
                MAIN_MISSION_CLEAR_TEXT,
                subMissionItems,
                FormatBattleTime(_missionRuntimeService.MissionProgress.ElapsedTime.Value),
                UNIMPLEMENTED_VALUE_TEXT,
                UNIMPLEMENTED_VALUE_TEXT,
                string.Empty);

            _stageResultViewModel.Apply(dto);
        }

        /// <summary>
        ///   ステージ敗北の結果を表示する。
        /// </summary>
        public void PresentDefeat()
        {
            StageResultDTO dto = new(
                StageResultType.Defeat,
                _selectedBattleStageState.StageName,
                _missionRuntimeService.MissionDefinition.MainMissionText,
                MAIN_MISSION_FAIL_TEXT,
                Array.Empty<StageResultMissionItemDTO>(),
                FormatBattleTime(_missionRuntimeService.MissionProgress.ElapsedTime.Value),
                UNIMPLEMENTED_VALUE_TEXT,
                string.Empty,
                SelectDefeatTip(_missionRuntimeService.MissionDefinition.DefeatTips));

            _stageResultViewModel.Apply(dto);
        }

        private readonly MissionRuntimeService _missionRuntimeService;
        private readonly SelectedBattleStageState _selectedBattleStageState;
        private readonly IStageResultViewModel _stageResultViewModel;

        /// <summary>
        ///     サブミッションの結果をDTOに変換する。
        /// </summary>
        /// <param name="evaluationResult">ミッション評価の結果。</param>
        /// <returns>サブミッションのDTO配列。</returns>
        private static StageResultMissionItemDTO[] BuildSubMissionItems(
            MissionEvaluationResult evaluationResult)
        {
            MissionEvaluationProgress[] progresses = evaluationResult.Progresses;

            StageResultMissionItemDTO[] items = new StageResultMissionItemDTO[progresses.Length];

            for (int i = 0; i < progresses.Length; i++)
            {
                items[i] = new StageResultMissionItemDTO(
                    progresses[i].Description,
                    progresses[i].IsSucceeded);
            }

            return items;
        }

        /// <summary>
        ///    経過時間を「mm:ss」形式の文字列に変換する。
        /// </summary>
        /// <param name="elapsedSeconds">経過時間（秒）。</param>
        /// <returns>「mm:ss」形式の文字列。</returns>
        private static string FormatBattleTime(float elapsedSeconds)
        {
            int totalSeconds = Mathf.Max(0, (int)Mathf.Floor(elapsedSeconds));

            int minutes = totalSeconds / SECOND_PER_MINUTE;
            int seconds = totalSeconds % SECOND_PER_MINUTE;

            return $"{minutes:00}:{seconds:00}";
        }

        /// <summary>
        ///     敗北時のヒントをランダムに選択する。
        /// </summary>
        /// <param name="defeatTips">敗北時のヒントのリスト。</param>
        /// <returns></returns>
        private string SelectDefeatTip(
            IReadOnlyList<string> defeatTips)
        {
            if (defeatTips == null || defeatTips.Count == 0)
            {
                return DEFAULT_DEFEAT_TIP_TEXT;
            }

            int selectedIndex = UnityEngine.Random.Range(0, defeatTips.Count);

            return defeatTips[selectedIndex];
        }
    }
}
