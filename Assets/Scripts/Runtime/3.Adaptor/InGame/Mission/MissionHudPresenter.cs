using KillChord.Runtime.Adaptor.Persistent.Localization;
using KillChord.Runtime.Application.InGame.Mission;
using KillChord.Runtime.Domain.InGame.Mission;
using KillChord.Runtime.Domain.InGame.Mission.ClearCondition;

namespace KillChord.Runtime.Adaptor.InGame.Mission
{
    /// <summary>
    ///     ミッションの情報をHUDに表示するためのプレゼンタークラス。
    /// </summary>
    public class MissionHudPresenter
    {
        /// <summary>
        ///     MissionHudPresenter クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="missionRuntimeService">ミッションランタイムサービス。</param>
        /// <param name="missionHudViewModel">ミッションHUDビューモデル。</param>
        /// <param name="textProvider">進捗回数の書式の取得元。nullなら日本語で表示する。</param>
        public MissionHudPresenter(
            MissionRuntimeService missionRuntimeService,
            IMissionHudViewModel missionHudViewModel,
            IUiTextProvider textProvider = null)
        {
            _missionRuntimeService = missionRuntimeService;
            _missionHudViewModel = missionHudViewModel;
            _textProvider = textProvider;
        }

        /// <summary>
        ///     情報を表示します。
        /// </summary>
        public void Present()
        {
            MissionEvaluationResult result = _missionRuntimeService.BuildEvaluationResult();

            MissionEvaluationItemDTO[] evaluationItems = new MissionEvaluationItemDTO[result.Progresses.Length];

            for (int i = 0; i < evaluationItems.Length; i++)
            {
                MissionEvaluationProgress progress = result.Progresses[i];
                evaluationItems[i] = new MissionEvaluationItemDTO(
                    progress.Description,
                    ConvertDisplayState(progress.DisplaySituation));
            }

            string resultText = _missionRuntimeService.MissionProgress.EndReason.ToString();
            string mainMissionText = GetMainMissionText();

            MissionHudDTO dto = new MissionHudDTO(
                mainMissionText,
                evaluationItems);

            _missionHudViewModel.Apply(dto);
        }

        private const string PROGRESS_COUNT_FORMAT_KEY = "ui.mission.progress_count_format";
        private const string PROGRESS_COUNT_FORMAT_FALLBACK = "{0}({1}/{2}回)";

        /// <summary> ミッションランタイムサービス。 </summary>
        private readonly MissionRuntimeService _missionRuntimeService;
        /// <summary> ミッションHUDビューモデル。 </summary>
        private readonly IMissionHudViewModel _missionHudViewModel;
        /// <summary> 進捗回数の書式の取得元。 </summary>
        private readonly IUiTextProvider _textProvider;

        /// <summary>
        ///     現在の進行状態に対応するメインミッション表示文を取得します。
        /// </summary>
        /// <returns> 表示するメインミッション文です。 </returns>
        private string GetMainMissionText()
        {
            int stepIndex = _missionRuntimeService.MissionProgress.ObjectiveStepIndex;
            ObjectiveSequenceStep step = _missionRuntimeService.MissionDefinition.ClearCondition.GetStep(stepIndex);
            if (string.IsNullOrWhiteSpace(step?.GuideMessageText))
            {
                return _missionRuntimeService.MissionDefinition.MainMissionText;
            }

            if (step.Condition is IObjectiveProgressReporter progressReporter
                && progressReporter.RequiredCount > 0)
            {
                int currentCount = progressReporter.CurrentCount(_missionRuntimeService.MissionProgress);
                return UiText.Format(
                    _textProvider,
                    PROGRESS_COUNT_FORMAT_KEY,
                    PROGRESS_COUNT_FORMAT_FALLBACK,
                    step.GuideMessageText,
                    currentCount,
                    progressReporter.RequiredCount);
            }

            return step.GuideMessageText;
        }

        /// <summary>
        ///     評価の表示状況を HUD 用の表示状態に変換する。
        /// </summary>
        private MissionEvaluationDisplayState ConvertDisplayState(
            MissionEvaluationDisplaySituation situation)
        {
            switch (situation)
            {
                case MissionEvaluationDisplaySituation.Failed:
                    return MissionEvaluationDisplayState.Failed;

                case MissionEvaluationDisplaySituation.Challenging:
                    return MissionEvaluationDisplayState.Challenging;

                case MissionEvaluationDisplaySituation.Succeeded:
                    return MissionEvaluationDisplayState.Succeeded;

                default:
                    return MissionEvaluationDisplayState.Challenging;
            }
        }
    }
}
