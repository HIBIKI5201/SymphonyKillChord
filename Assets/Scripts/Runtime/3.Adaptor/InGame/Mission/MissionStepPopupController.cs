using KillChord.Runtime.Application.InGame.Mission;
using KillChord.Runtime.Domain.InGame.Mission.ClearCondition;
using System;

namespace KillChord.Runtime.Adaptor.InGame.Mission
{
    /// <summary>
    ///     Missionの目標ステップと説明ポップアップ表示を仲介するControllerです。
    /// </summary>
    public sealed class MissionStepPopupController : IDisposable
    {
        /// <summary>
        ///     Missionの目標ステップと説明ポップアップ表示を結合します。
        /// </summary>
        /// <param name="missionRuntimeService"> Missionのランタイムサービスです。 </param>
        /// <param name="objectiveSequence"> ステップを含む目標シーケンスです。 </param>
        /// <param name="popupView"> 説明ポップアップの抽象です。 </param>
        public MissionStepPopupController(
            MissionRuntimeService missionRuntimeService,
            ObjectiveSequenceClearCondition objectiveSequence,
            IMissionStepPopupView popupView)
        {
            _missionRuntimeService = missionRuntimeService
                ?? throw new ArgumentNullException(nameof(missionRuntimeService));
            _objectiveSequence = objectiveSequence
                ?? throw new ArgumentNullException(nameof(objectiveSequence));
            _popupView = popupView
                ?? throw new ArgumentNullException(nameof(popupView));

            _missionRuntimeService.OnObjectiveStepChanged += HandleObjectiveStepChanged;
        }

        /// <summary>
        ///     Missionイベントの購読を解除します。
        /// </summary>
        public void Dispose()
        {
            _missionRuntimeService.OnObjectiveStepChanged -= HandleObjectiveStepChanged;
        }

        private readonly MissionRuntimeService _missionRuntimeService;
        private readonly ObjectiveSequenceClearCondition _objectiveSequence;
        private readonly IMissionStepPopupView _popupView;

        /// <summary>
        ///     目標ステップの変化に応じて説明ポップアップの表示を切り替えます。
        /// </summary>
        /// <param name="stepIndex"> 開始した目標ステップのIndexです。 </param>
        private void HandleObjectiveStepChanged(int stepIndex)
        {
            ObjectiveSequenceStep step = _objectiveSequence.GetStep(stepIndex);
            PopupClearCondition popupCondition = step != null ? ClearConditionChain.Find<PopupClearCondition>(step.Condition) : null;
            if (popupCondition == null)
            {
                _popupView.Hide();
                return;
            }

            _popupView.Show(step.GuideMessageText, popupCondition.PopupImage);
        }
    }
}
