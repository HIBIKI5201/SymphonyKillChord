using KillChord.Runtime.Application.InGame.Mission;
using KillChord.Runtime.Domain.InGame.Mission.ClearCondition;
using KillChord.Runtime.Domain.InGame.Mission.StepEntryAction;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Mission
{
    /// <summary>
    ///     Missionの目標ステップ開始と、設定された進入時アクションの実行を仲介するControllerです。
    /// </summary>
    public sealed class MissionStepEntryActionController : IDisposable
    {
        /// <summary>
        ///     Missionの目標ステップ開始と進入時アクションを結合します。
        /// </summary>
        /// <param name="missionRuntimeService">Missionのランタイムサービスです。</param>
        /// <param name="objectiveSequence">進入時アクションを持つ目標シーケンスです。</param>
        /// <param name="actionExecutors">進入時アクションの実行器一覧です。</param>
        public MissionStepEntryActionController(
            MissionRuntimeService missionRuntimeService,
            ObjectiveSequenceClearCondition objectiveSequence,
            IReadOnlyList<IMissionStepEntryActionExecutor> actionExecutors)
        {
            _missionRuntimeService = missionRuntimeService
                ?? throw new ArgumentNullException(nameof(missionRuntimeService));
            _objectiveSequence = objectiveSequence
                ?? throw new ArgumentNullException(nameof(objectiveSequence));
            _actionExecutors = CreateActionExecutorMap(actionExecutors);
            _lastHandledStepIndex = -1;

            _missionRuntimeService.OnObjectiveStepChanged += HandleObjectiveStepChanged;
            HandleObjectiveStepChanged(_missionRuntimeService.MissionProgress.ObjectiveStepIndex);
        }

        /// <summary>
        ///     Missionイベントの購読を解除します。
        /// </summary>
        public void Dispose()
        {
            _missionRuntimeService.OnObjectiveStepChanged -= HandleObjectiveStepChanged;
        }

        /// <summary> Missionのランタイムサービスです。 </summary>
        private readonly MissionRuntimeService _missionRuntimeService;
        /// <summary> 進入時アクションを持つ目標シーケンスです。 </summary>
        private readonly ObjectiveSequenceClearCondition _objectiveSequence;
        /// <summary> アクション型をキーとした実行器一覧です。 </summary>
        private readonly Dictionary<Type, IMissionStepEntryActionExecutor> _actionExecutors;
        /// <summary> 進入時アクションを実行済みの最新ステップIndexです。 </summary>
        private int _lastHandledStepIndex;

        /// <summary>
        ///     目標ステップが開始されたとき、その進入時アクションを一度だけ実行します。
        /// </summary>
        /// <param name="stepIndex">開始した目標ステップのIndexです。</param>
        private void HandleObjectiveStepChanged(int stepIndex)
        {
            if (_lastHandledStepIndex == stepIndex)
            {
                return;
            }

            _lastHandledStepIndex = stepIndex;
            ObjectiveSequenceStep step = _objectiveSequence.GetStep(stepIndex);
            if (step == null)
            {
                return;
            }

            for (int i = 0; i < step.EntryActions.Count; i++)
            {
                IMissionStepEntryAction entryAction = step.EntryActions[i];
                if (!_actionExecutors.TryGetValue(entryAction.GetType(), out IMissionStepEntryActionExecutor executor))
                {
                    Debug.LogWarning(
                        $"[{nameof(MissionStepEntryActionController)}] " +
                        $"{entryAction.GetType().Name} の実行器が登録されていません。");
                    continue;
                }

                executor.Execute(entryAction);
            }
        }

        /// <summary>
        ///     アクション型をキーとした実行器一覧を生成します。
        /// </summary>
        /// <param name="actionExecutors">実行器一覧です。</param>
        /// <returns>アクション型をキーとした実行器一覧です。</returns>
        private static Dictionary<Type, IMissionStepEntryActionExecutor> CreateActionExecutorMap(
            IReadOnlyList<IMissionStepEntryActionExecutor> actionExecutors)
        {
            if (actionExecutors == null)
            {
                throw new ArgumentNullException(nameof(actionExecutors));
            }

            Dictionary<Type, IMissionStepEntryActionExecutor> executors = new(actionExecutors.Count);
            for (int i = 0; i < actionExecutors.Count; i++)
            {
                IMissionStepEntryActionExecutor executor = actionExecutors[i];
                if (executor == null)
                {
                    throw new ArgumentException(
                        $"{nameof(actionExecutors)}[{i}] must not be null.",
                        nameof(actionExecutors));
                }

                if (executor.EntryActionType == null)
                {
                    throw new ArgumentException(
                        $"{nameof(actionExecutors)}[{i}].{nameof(executor.EntryActionType)} must not be null.",
                        nameof(actionExecutors));
                }

                if (!executors.TryAdd(executor.EntryActionType, executor))
                {
                    throw new ArgumentException(
                        $"{executor.EntryActionType.Name} is registered more than once.",
                        nameof(actionExecutors));
                }
            }

            return executors;
        }
    }
}
