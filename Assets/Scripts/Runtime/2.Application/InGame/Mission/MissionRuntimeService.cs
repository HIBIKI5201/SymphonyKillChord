using KillChord.Runtime.Domain.InGame.Mission;
using KillChord.Runtime.Domain.InGame.Mission.ClearCondition;
using System;
using System.Collections.Generic;

namespace KillChord.Runtime.Application.InGame.Mission
{
    /// <summary>
    ///     ミッションのランタイムにおける状態と処理を管理するサービスクラス。
    /// </summary>
    public class MissionRuntimeService
    {
        /// <summary>
        ///     MissionRuntimeService クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="missionDefinition">ミッション定義。</param>
        /// <param name="missionProgress">進行状況。</param>
        /// <param name="missionTimeAdvanceUseCase">時間経過ユースケース。</param>
        /// <param name="missionEnemyKilledUseCase">敵撃破ユースケース。</param>
        /// <param name="missionActionPerformedUseCase">プレイヤー行動発動ユースケース。</param>
        /// <param name="missionPlayerDeadUseCase">プレイヤー死亡ユースケース。</param>
        /// <param name="missionRuleRunner">ルール評価器。</param>
        /// <param name="missionEvaluationRunner">評価実行器。</param>
        public MissionRuntimeService(
            MissionDefinition missionDefinition,
            MissionProgress missionProgress,
            MissionTimeAdvanceUsecase missionTimeAdvanceUseCase,
            MissionEnemyKilledUsecase missionEnemyKilledUseCase,
            MissionActionPerformedUsecase missionActionPerformedUseCase,
            MissionPlayerDeadUsecase missionPlayerDeadUseCase,
            MissionRuleRunner missionRuleRunner,
            MissionEvaluationRunner missionEvaluationRunner)
        {
            _missionDefinition = missionDefinition;
            _missionProgress = missionProgress;
            _missionTimeAdvanceUseCase = missionTimeAdvanceUseCase;
            _missionEnemyKilledUseCase = missionEnemyKilledUseCase;
            _missionActionPerformedUseCase = missionActionPerformedUseCase;
            _missionPlayerDeadUseCase = missionPlayerDeadUseCase;
            _missionRuleRunner = missionRuleRunner;
            _missionEvaluationRunner = missionEvaluationRunner;
            _lastObjectiveStepIndex = -1;

            _missionDefinition.ClearCondition.Start(_missionProgress);
        }

        /// <summary> ミッション終了イベント。 </summary>
        public event Action<MissionEndReason> OnMissionFinished;

        /// <summary>
        ///     クリア条件の現在ステップが変化したときに発火します。
        /// </summary>
        public event Action<int> OnObjectiveStepChanged;

        /// <summary> ミッション定義を取得します。 </summary>
        public MissionDefinition MissionDefinition => _missionDefinition;
        /// <summary> 進行状況を取得します。 </summary>
        public MissionProgress MissionProgress => _missionProgress;

        /// <summary>
        ///     定期更新処理を行います。
        /// </summary>
        /// <param name="deltaTime">経過時間。</param>
        public void Tick(float deltaTime)
        {
            if (_missionProgress.IsFinished)
            {
                return;
            }
            _missionTimeAdvanceUseCase.Execute(_missionProgress, deltaTime);
            CheckObjectiveStepChanged();
            _missionRuleRunner.Evaluate(_missionProgress);
            CheckMissionFinished();
        }

        /// <summary>
        ///     敵が撃破された際の処理を行います。
        /// </summary>
        /// <param name="enemyMissionKey">敵のキー。</param>
        public void OnEnemyKilled(EnemyMissionKey enemyMissionKey)
        {
            if (_missionProgress.IsFinished)
            {
                return;
            }
            _missionEnemyKilledUseCase.Execute(_missionProgress, enemyMissionKey);
            CheckObjectiveStepChanged();
            _missionRuleRunner.Evaluate(_missionProgress);
            CheckMissionFinished();
        }

        /// <summary>
        ///     プレイヤー行動が発動した際の処理を行います。
        /// </summary>
        /// <param name="actionKind">発動した行動の種別。</param>
        public void OnActionPerformed(MissionActionKind actionKind)
        {
            if (_missionProgress.IsFinished)
            {
                return;
            }
            _missionActionPerformedUseCase.Execute(_missionProgress, actionKind);
            CheckObjectiveStepChanged();
            _missionRuleRunner.Evaluate(_missionProgress);
            CheckMissionFinished();
        }

        /// <summary>
        ///     ひとつの操作から発生した複数のプレイヤー行動を、まとめて処理します。
        ///     <para>
        ///         1回の攻撃は拍子付きの行動と汎用の<see cref="MissionActionKind.Attack"/>の2件を発生させます。
        ///         これらを個別に処理すると、1件目でステップが進んだ場合に2件目が次のステップへ計上され、
        ///         操作していないのに次ステップが即座に達成されてしまいます。
        ///         全件を記録してから一度だけ進行判定を行うことで、この伝播を防ぎます。
        ///     </para>
        /// </summary>
        /// <param name="actionKinds"> 同時に発動した行動の種別一覧です。 </param>
        public void OnActionsPerformed(IReadOnlyList<MissionActionKind> actionKinds)
        {
            if (_missionProgress.IsFinished || actionKinds == null || actionKinds.Count == 0)
            {
                return;
            }

            for (int i = 0; i < actionKinds.Count; i++)
            {
                _missionActionPerformedUseCase.Execute(_missionProgress, actionKinds[i]);
            }

            CheckObjectiveStepChanged();
            _missionRuleRunner.Evaluate(_missionProgress);
            CheckMissionFinished();
        }

        /// <summary>
        ///     プレイヤーが死亡した際の処理を行います。
        /// </summary>
        public void OnPlayerDead()
        {
            if (_missionProgress.IsFinished)
            {
                return;
            }
            _missionPlayerDeadUseCase.Execute(_missionProgress);
            CheckObjectiveStepChanged();
            _missionRuleRunner.Evaluate(_missionProgress);
            CheckMissionFinished();
        }

        /// <summary>
        ///     評価結果を構築します。
        /// </summary>
        /// <returns>評価結果。</returns>
        public MissionEvaluationResult BuildEvaluationResult()
        {
            return _missionEvaluationRunner.Run(
                _missionProgress,
                _missionDefinition.EvaluationConditions);
        }

        /// <summary> ミッション定義。 </summary>
        private readonly MissionDefinition _missionDefinition;
        /// <summary> 進行状況。 </summary>
        private readonly MissionProgress _missionProgress;
        /// <summary> 時間経過ユースケース。 </summary>
        private readonly MissionTimeAdvanceUsecase _missionTimeAdvanceUseCase;
        /// <summary> 敵撃破ユースケース。 </summary>
        private readonly MissionEnemyKilledUsecase _missionEnemyKilledUseCase;
        /// <summary> プレイヤー行動発動ユースケース。 </summary>
        private readonly MissionActionPerformedUsecase _missionActionPerformedUseCase;
        /// <summary> プレイヤー死亡ユースケース。 </summary>
        private readonly MissionPlayerDeadUsecase _missionPlayerDeadUseCase;
        /// <summary> ルール評価器。 </summary>
        private readonly MissionRuleRunner _missionRuleRunner;
        /// <summary> 評価実行器。 </summary>
        private readonly MissionEvaluationRunner _missionEvaluationRunner;

        /// <summary> 目標シーケンスの直前のステップIndex。変化検知に使用する。 </summary>
        private int _lastObjectiveStepIndex;

        /// <summary>
        ///    ミッションが終了しているかどうかをチェックし、終了している場合はイベントを発火させます。
        ///    全ての進行APIはメソッド先頭で<see cref="MissionProgress.IsFinished"/>を確認して即returnするため、
        ///    このメソッドが終了検知後に再度到達することはなく、イベントは一度しか発火しません。
        /// </summary>
        private void CheckMissionFinished()
        {
            if (_missionProgress.IsFinished)
            {
                OnMissionFinished?.Invoke(_missionProgress.EndReason);
            }
        }

        /// <summary>
        ///     クリア条件の現在ステップを進め、変化を検知してイベントを発火させます。
        /// </summary>
        private void CheckObjectiveStepChanged()
        {
            _missionDefinition.ClearCondition.TryAdvance(_missionProgress);

            int currentStepIndex = _missionProgress.ObjectiveStepIndex;
            if (currentStepIndex == _lastObjectiveStepIndex)
            {
                return;
            }

            _lastObjectiveStepIndex = currentStepIndex;
            OnObjectiveStepChanged?.Invoke(currentStepIndex);
        }
    }
}
