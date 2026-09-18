using KillChord.Runtime.Adaptor.InGame.StageSelect;
using KillChord.Runtime.Application.InGame.Mission;
using KillChord.Runtime.Domain.InGame.Mission;
using KillChord.Runtime.Domain.InGame.Mission.ClearCondition;
using KillChord.Runtime.Utility.Persistent;
using System;

namespace KillChord.Runtime.Adaptor.InGame.Mission
{
    /// <summary>
    ///     チュートリアルのスキル課題中に、成立したスキルの成功表示を通知します。
    /// </summary>
    public sealed class TutorialSkillFeedbackPresenter : IDisposable
    {
        /// <summary>
        ///     現在のミッションと表示先を受け取り、スキル発動通知を購読します。
        /// </summary>
        /// <param name="missionProvider"> 現在有効なミッションを取得します。 </param>
        /// <param name="selectedStage"> チュートリアル判定に使う選択中ステージです。 </param>
        /// <param name="view"> 成功表示を行うViewです。 </param>
        public TutorialSkillFeedbackPresenter(
            Func<MissionRuntimeService> missionProvider,
            SelectedBattleStageState selectedStage,
            ITutorialAttackFeedbackView view)
        {
            _missionProvider = missionProvider ?? throw new ArgumentNullException(nameof(missionProvider));
            _selectedStage = selectedStage ?? throw new ArgumentNullException(nameof(selectedStage));
            _view = view ?? throw new ArgumentNullException(nameof(view));
            EventBus<EOnSkillExecuted>.Register(HandleSkillExecutedHandler);
        }

        /// <summary>
        ///     スキル通知の購読を解除します。複数回呼び出しても安全です。
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            EventBus<EOnSkillExecuted>.Unregister(HandleSkillExecutedHandler);
        }

        private readonly Func<MissionRuntimeService> _missionProvider;
        private readonly SelectedBattleStageState _selectedStage;
        private readonly ITutorialAttackFeedbackView _view;
        private bool _isDisposed;

        /// <summary>
        ///     ミッション進行前にスキル課題を判定し、実際に成立した発動だけ表示します。
        /// </summary>
        /// <param name="eventData"> 成立したスキルの通知です。 </param>
        private void HandleSkillExecutedHandler(EOnSkillExecuted eventData)
        {
            if (_isDisposed || !_selectedStage.HasSelectedBattleStage
                || !_selectedStage.CurrentStageDefinition.IsTutorial)
            {
                return;
            }

            MissionRuntimeService mission = _missionProvider.Invoke();
            if (mission == null || mission.MissionProgress.IsFinished)
            {
                return;
            }

            ObjectiveSequenceStep step = mission.MissionDefinition.ClearCondition
                .GetStep(mission.MissionProgress.ObjectiveStepIndex);
            if (step == null || ClearConditionChain.Contains<PopupClearCondition>(step.Condition))
            {
                return;
            }

            ActionRepeatCountClearCondition condition =
                ClearConditionChain.Find<ActionRepeatCountClearCondition>(step.Condition);
            if (condition == null || !condition.TargetsAction(MissionActionKind.Skill))
            {
                return;
            }

            // この通知はSkillControllerのミッション記録より先なので、最後の発動も表示できる。
            _view.ShowFeedback(true);
        }
    }
}