using KillChord.Runtime.Application.InGame.Mission;
using KillChord.Runtime.Domain.InGame.Mission;

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
        public MissionHudPresenter(MissionRuntimeService missionRuntimeService, IMissionHudViewModel missionHudViewModel)
        {
            _missionRuntimeService = missionRuntimeService;
            _missionHudViewModel = missionHudViewModel;
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

            MissionHudDTO dto = new MissionHudDTO(
                _missionRuntimeService.MissionDefinition.MainMissionText,
                resultText,
                evaluationItems);

            _missionHudViewModel.Apply(dto);
        }

        /// <summary> ミッションランタイムサービス。 </summary>
        private readonly MissionRuntimeService _missionRuntimeService;
        /// <summary> ミッションHUDビューモデル。 </summary>
        private readonly IMissionHudViewModel _missionHudViewModel;

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
