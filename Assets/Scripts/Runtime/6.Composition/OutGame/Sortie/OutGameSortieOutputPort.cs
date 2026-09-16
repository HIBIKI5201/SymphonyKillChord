using KillChord.Runtime.Adaptor.InGame.Mission;
using KillChord.Runtime.Adaptor.InGame.StageSelect;
using KillChord.Runtime.Adaptor.OutGame.Scenario;
using KillChord.Runtime.Adaptor.OutGame.StageSelect;
using KillChord.Runtime.Adaptor.Persistent.SceneManagement;
using KillChord.Runtime.Application.OutGame.Sortie;
using KillChord.Runtime.Application.Persistent.Load;
using KillChord.Runtime.Composition.Persistent.Input;
using KillChord.Runtime.Composition.Persistent.SceneManagement;
using KillChord.Runtime.Domain.OutGame.StageSelect;
using KillChord.Runtime.View.OutGame.Screen;
using KillChord.Runtime.View.Persistent.Input;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KillChord.Runtime.Composition.OutGame.Sortie
{
    /// <summary>
    ///     出撃ユースケースの出力ポートの実装クラス。
    ///     Application と View に依存しているため Composition に配置している。
    /// </summary>
    public sealed class OutGameSortieOutputPort : IOutGameSortieOutputPort
    {
        /// <summary>
        ///    OutGameSortieOutputPort を初期化します。
        /// </summary>
        /// <param name="outGameUIEvent"> OutGameUIEvent のインスタンス。 </param>
        /// <param name="inputComposition"> InputComposition のインスタンス。 </param>
        public OutGameSortieOutputPort(
            OutGameUIEvent outGameUIEvent,
            InputComposition inputComposition,
            SceneTransitionController transition,
            ILoadingOperationExecutor executor,
            SceneTransitionInitializer transitionInitializer)
        {
            _outGameUIEvent = outGameUIEvent;
            _inputComposition = inputComposition;
            _transition = transition;
            _executor = executor;
            _transitionInitializer = transitionInitializer;
        }

        /// <summary>
        ///     戦闘準備画面の表示を要求します。
        /// </summary>
        public void ShowBattlePreparationScreen()
        {
            _outGameUIEvent.OnShownBattlePreparationScreen?.Invoke();
        }

        /// <summary>
        ///     バトル開始イベントを通知します。
        /// </summary>
        public void StartBattle()
        {
            _outGameUIEvent.OnStartGame?.Invoke();
        }

        /// <summary>
        ///     ホーム画面の表示を要求します。
        /// </summary>
        public void ShowHomeScreen()
        {
            _outGameUIEvent.OnShownHomeScreen?.Invoke();
        }

        /// <summary>
        ///     シナリオ再生状態に合わせてOutGameの表示と入力を切り替えます。
        /// </summary>
        /// <param name="isActive"> OutGameを有効にする場合はtrueです。 </param>
        public void SetOutGameActiveForScenario(bool isActive)
        {
            _outGameUIEvent.OnOutGameUiVisibilityChanged?.Invoke(isActive);

            if (_inputComposition == null)
            {
                return;
            }

            if (isActive)
            {
                _inputComposition.GetInputMapController.EnableCommonWith(InputMapNames.OutGame);
                return;
            }

            _inputComposition.GetInputMapController.EnableCommonWith(InputMapNames.Scenario);
        }

        /// <summary>
        ///     初回Build順を変えず、Readyで公開済みStateへ接続します。
        /// </summary>
        public void ConnectScenarioBattleSortie(PendingNodeTransitionState pendingState,
            SelectedScenarioState selectedState, BattleSortieSelectionService selectionService)
        {
            _pendingState = pendingState;
            _selectedState = selectedState;
            _selectionService = selectionService;
        }

        /// <summary>
        ///     同期受付の内側で予約を消費し、両旧シーンを終了して出撃します。
        /// </summary>
        public async Task<ScenarioBattleSortieResult> StartBattleFromScenarioAsync(
            string scenarioSceneName, string outGameSceneName, BattleStageDefinition definition,
            int scenarioSelectionRevision, bool isInputValid)
        {
            PendingNodeTransitionState pending = _pendingState;
            SelectedScenarioState selected = _selectedState;
            if (_executor.IsSessionActive || !_transition.TryBeginScenarioBattleSortie(definition?.TargetSceneName))
            {
                return ScenarioBattleSortieResult.Busy;
            }

            bool hasConsumedReservation = false;
            bool hasBegunSceneTransition = false;
            bool isComplete = false;
            try
            {
                if (pending == null
                    || !pending.TryPeekCompleted(out PendingNodeTransition candidate)
                    || !ReferenceEquals(candidate.TargetStageDefinition, definition)
                    || !string.Equals(candidate.ReturnSceneName, outGameSceneName, StringComparison.Ordinal))
                {
                    return ScenarioBattleSortieResult.Busy;
                }

                string persistentSceneName = _transitionInitializer.PersistentSceneName;
                bool hasValidSceneNames = HasDistinctSceneNames(
                    scenarioSceneName, outGameSceneName, persistentSceneName,
                    definition?.TargetSceneName, definition?.BattleSceneName);
                if (hasValidSceneNames
                    && (IsLoaded(definition.TargetSceneName) || IsLoaded(definition.BattleSceneName)))
                {
                    return ScenarioBattleSortieResult.Busy;
                }

                // 受付から予約消費・選択準備までは await を挟まず、新規要求を受け付けません。
                if (!pending.TryConsumeCompleted(out _)) { return ScenarioBattleSortieResult.Busy; }
                hasConsumedReservation = true;
                if (!isInputValid || selected == null || _selectionService == null
                    || !hasValidSceneNames
                    || !IsLoaded(scenarioSceneName) || !IsLoaded(outGameSceneName)
                    || !IsLoaded(persistentSceneName)
                    || !_selectionService.TryPrepareBattleSortie(definition, outGameSceneName))
                {
                    ClearFailedSelection(pending, definition);
                    return ScenarioBattleSortieResult.PreparationFailed;
                }

                hasBegunSceneTransition = true;
                try
                {
                    bool success = await _transition.UnloadSourcesThenLoadSceneKeepingLoadingWithPersistentLifetimeAsync(
                        scenarioSceneName, outGameSceneName, persistentSceneName, definition.TargetSceneName);
                    if (success && IsLoaded(definition.TargetSceneName)
                        && await _transition.SettleScenarioBattleInitializationAsync(
                            false, IsLoaded(definition.TargetSceneName)))
                    {
                        selected.TryClear(scenarioSelectionRevision);
                        isComplete = true;
                        return ScenarioBattleSortieResult.Started;
                    }
                }
                catch (OperationCanceledException) when (_transition.PersistentLifetimeToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }

                _transition.PersistentLifetimeToken.ThrowIfCancellationRequested();
                _transition.StopScenarioBattleInitialization();
                selected.TryClear(scenarioSelectionRevision);
                _transitionInitializer.ShowScenarioBattleRecovery(definition.TargetSceneName,
                    scenarioSceneName, definition.BattleSceneName, definition.TargetSceneName, outGameSceneName);
                return ScenarioBattleSortieResult.Failed;
            }
            catch (OperationCanceledException) when (_transition.PersistentLifetimeToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                // 遷移開始後の例外は通常帰還へ合流させず、専用受付を保持します。
                if (hasBegunSceneTransition) { throw; }
                Debug.LogException(exception);
                if (!hasConsumedReservation) { return ScenarioBattleSortieResult.Busy; }
                ClearFailedSelection(pending, definition);
                return ScenarioBattleSortieResult.PreparationFailed;
            }
            finally
            {
                if (!hasBegunSceneTransition || isComplete || _transition.PersistentLifetimeToken.IsCancellationRequested)
                {
                    _transition.EndScenarioBattleSortie();
                }
            }
        }

        private readonly OutGameUIEvent _outGameUIEvent;
        private readonly InputComposition _inputComposition;
        private readonly SceneTransitionController _transition;
        private readonly ILoadingOperationExecutor _executor;
        private readonly SceneTransitionInitializer _transitionInitializer;
        private PendingNodeTransitionState _pendingState;
        private SelectedScenarioState _selectedState;
        private BattleSortieSelectionService _selectionService;

        /// <summary>
        ///     正規受付中の準備失敗で、予約と一致する戦闘選択を整理します。
        /// </summary>
        private static void ClearFailedSelection(PendingNodeTransitionState pending, BattleStageDefinition definition)
        {
            pending?.Clear();
            if (ServiceLocator.TryGetInstance(out SelectedBattleStageState battle)
                && battle.HasSelectedBattleStage && ReferenceEquals(battle.CurrentStageDefinition, definition))
            {
                battle.Clear();
                if (ServiceLocator.TryGetInstance(out SelectedMissionState mission)
                    && mission.HasSelectedMission && mission.CurrentMissionId.Value == definition.MissionId.Value)
                {
                    mission.Clear();
                }
            }
        }

        /// <summary>
        ///     専用出撃が操作するシーン名の空文字と重複を確認します。
        /// </summary>
        private static bool HasDistinctSceneNames(params string[] sceneNames)
        {
            for (int index = 0; index < sceneNames.Length; index++)
            {
                if (string.IsNullOrWhiteSpace(sceneNames[index])) { return false; }
                for (int previous = 0; previous < index; previous++)
                {
                    if (string.Equals(sceneNames[index], sceneNames[previous], StringComparison.Ordinal)) { return false; }
                }
            }
            return true;
        }

        /// <summary>
        ///     実際にロード済みのシーンだけを判定します。
        /// </summary>
        private static bool IsLoaded(string sceneName)
        {
            return !string.IsNullOrWhiteSpace(sceneName) && SceneManager.GetSceneByName(sceneName).isLoaded;
        }
    }
}
