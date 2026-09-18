using KillChord.Runtime.Adaptor.InGame.Battle;
using KillChord.Runtime.Adaptor.InGame.StageSelect;
using KillChord.Runtime.Application.InGame.Mission;
using KillChord.Runtime.Domain.InGame.Mission.ClearCondition;
using System;

namespace KillChord.Runtime.Adaptor.InGame.Mission
{
    /// <summary>
    ///     成立した攻撃を現在の色指定課題と比較し、表示結果だけをViewへ通知します。
    /// </summary>
    public sealed class TutorialAttackFeedbackPresenter : IDisposable
    {
        /// <summary>
        ///     現在のミッションを取得する窓口と表示先を受け取り、攻撃成立通知を購読します。
        /// </summary>
        /// <param name="attackSignal"> ミッション進行前に通知される攻撃成立Signalです。 </param>
        /// <param name="missionRuntimeServiceProvider"> 現在有効なミッションを取得します。 </param>
        /// <param name="selectedBattleStageState"> チュートリアル判定に使う選択中ステージです。 </param>
        /// <param name="view"> 成功・失敗の表示先です。 </param>
        public TutorialAttackFeedbackPresenter(
            IPlayerAttackSignal attackSignal,
            Func<MissionRuntimeService> missionRuntimeServiceProvider,
            SelectedBattleStageState selectedBattleStageState,
            ITutorialAttackFeedbackView view)
        {
            _attackSignal = attackSignal ?? throw new ArgumentNullException(nameof(attackSignal));
            _missionRuntimeServiceProvider = missionRuntimeServiceProvider
                ?? throw new ArgumentNullException(nameof(missionRuntimeServiceProvider));
            _selectedBattleStageState = selectedBattleStageState
                ?? throw new ArgumentNullException(nameof(selectedBattleStageState));
            _view = view ?? throw new ArgumentNullException(nameof(view));

            _attackSignal.OnAttackExecuted += HandleAttackExecutedHandler;
        }

        /// <summary>
        ///     攻撃成立通知の購読を解除します。複数回呼び出しても安全です。
        /// </summary>
        public void Dispose()
        {
            if (_attackSignal == null)
            {
                return;
            }

            _attackSignal.OnAttackExecuted -= HandleAttackExecutedHandler;
            _attackSignal = null;
        }

        private readonly Func<MissionRuntimeService> _missionRuntimeServiceProvider;
        private readonly SelectedBattleStageState _selectedBattleStageState;
        private readonly ITutorialAttackFeedbackView _view;
        private IPlayerAttackSignal _attackSignal;

        /// <summary>
        ///     ミッション進行前の色指定と攻撃拍種を比較し、攻撃1回につき1回だけ表示します。
        /// </summary>
        /// <param name="beatCount"> 成立した攻撃の拍種です。 </param>
        /// <param name="isJustHit"> ジャスト判定。色課題では使用しません。 </param>
        private void HandleAttackExecutedHandler(int beatCount, bool isJustHit)
        {
            if (!_selectedBattleStageState.HasSelectedBattleStage
                || !_selectedBattleStageState.CurrentStageDefinition.IsTutorial)
            {
                return;
            }

            MissionRuntimeService mission = _missionRuntimeServiceProvider.Invoke();
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
            if (condition == null || !condition.TargetBeatType.HasValue)
            {
                return;
            }

            // 後続のミッション記録で最終成功が次ステップへ進む前に、表示する結果を確定する。
            _view.ShowFeedback(beatCount == (int)condition.TargetBeatType.Value);
        }
    }
}
