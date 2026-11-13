using KillChord.Runtime.Adaptor.InGame.Mission;
using KillChord.Runtime.Adaptor.InGame.StageSelect;
using KillChord.Runtime.Adaptor.OutGame.Scenario;
using KillChord.Runtime.Adaptor.OutGame.StageSelect;
using KillChord.Runtime.Adaptor.Persistent.SceneManagement;
using KillChord.Runtime.Application.OutGame.Sortie;
using KillChord.Runtime.Application.Persistent.Load;
using KillChord.Runtime.Composition.Persistent.SceneManagement;
using KillChord.Runtime.Domain.OutGame.StageSelect;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KillChord.Runtime.Composition.OutGame.Sortie
{
    /// <summary>
    ///     シナリオ完了後に予約済みのバトルステージへ直接出撃する処理です。
    ///     <para>
    ///         OutGame のUI初期化から切り離しており、OutGame の上で再生したシナリオと、
    ///         タイトルから直接始めたシナリオ（OutGame が無い）の両方から使う。
    ///         予約の消費、二重出撃の防止、戦闘・ミッションの選択、ロード待ち、失敗時の復旧をまとめて扱う。
    ///     </para>
    /// </summary>
    public sealed class ScenarioBattleSortieExecutor
    {
        /// <summary>
        ///     出撃に使う常駐サービスを指定して生成します。
        /// </summary>
        /// <param name="transition"> シーン遷移コントローラー。 </param>
        /// <param name="executor"> ロード処理の実行者。 </param>
        /// <param name="transitionInitializer"> 常駐シーン名と失敗時の復旧表示を持つ初期化モジュール。 </param>
        public ScenarioBattleSortieExecutor(
            SceneTransitionController transition,
            ILoadingOperationExecutor executor,
            SceneTransitionInitializer transitionInitializer)
        {
            _transition = transition ?? throw new ArgumentNullException(nameof(transition));
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
            _transitionInitializer = transitionInitializer
                ?? throw new ArgumentNullException(nameof(transitionInitializer));
        }

        /// <summary>
        ///     予約済みのバトルステージへ出撃します。
        /// </summary>
        /// <param name="pending"> 予約済み遷移の状態。 </param>
        /// <param name="selected"> 選択中シナリオの状態。 </param>
        /// <param name="selectionService"> 戦闘・ミッションを選択するサービス。 </param>
        /// <param name="scenarioSceneName"> 終了するシナリオシーン名。 </param>
        /// <param name="outGameSceneName"> 終了する OutGame シーン名。OutGame を経由していない場合は空。 </param>
        /// <param name="returnSceneName"> 戦闘終了後の帰還先シーン名。 </param>
        /// <param name="definition"> 出撃するバトルステージ定義。 </param>
        /// <param name="scenarioSelectionRevision"> 出撃要求時のシナリオ選択の改訂番号。 </param>
        /// <param name="isInputValid"> 呼び出し側で検証した入力が有効な場合はtrue。 </param>
        /// <returns> 出撃の結果。 </returns>
        public async Task<ScenarioBattleSortieResult> ExecuteAsync(
            PendingNodeTransitionState pending, SelectedScenarioState selected,
            BattleSortieSelectionService selectionService, string scenarioSceneName, string outGameSceneName,
            string returnSceneName, BattleStageDefinition definition, int scenarioSelectionRevision, bool isInputValid)
        {
            if (_executor.IsSessionActive || !_transition.TryBeginScenarioBattleSortie(definition?.TargetSceneName))
            {
                return ScenarioBattleSortieResult.Busy;
            }

            bool hasOutGame = !string.IsNullOrWhiteSpace(outGameSceneName);
            bool hasConsumedReservation = false;
            bool hasBegunSceneTransition = false;
            bool isComplete = false;
            try
            {
                if (pending == null
                    || !pending.TryPeekCompleted(out PendingNodeTransition candidate)
                    || !ReferenceEquals(candidate.TargetStageDefinition, definition)
                    || !string.Equals(candidate.ReturnSceneName, returnSceneName, StringComparison.Ordinal))
                {
                    return ScenarioBattleSortieResult.Busy;
                }

                string persistentSceneName = _transitionInitializer.PersistentSceneName;
                bool hasValidSceneNames = HasDistinctSceneNames(
                    hasOutGame
                        ? new[] { scenarioSceneName, outGameSceneName, persistentSceneName,
                            definition?.TargetSceneName, definition?.BattleSceneName }
                        : new[] { scenarioSceneName, persistentSceneName,
                            definition?.TargetSceneName, definition?.BattleSceneName });
                if (hasValidSceneNames
                    && (IsLoaded(definition.TargetSceneName) || IsLoaded(definition.BattleSceneName)))
                {
                    return ScenarioBattleSortieResult.Busy;
                }

                // 受付から予約消費・選択準備までは await を挟まず、新規要求を受け付けません。
                if (!pending.TryConsumeCompleted(out _)) { return ScenarioBattleSortieResult.Busy; }
                hasConsumedReservation = true;
                if (!isInputValid || selected == null || selectionService == null
                    || !hasValidSceneNames
                    || string.IsNullOrWhiteSpace(returnSceneName)
                    || !IsLoaded(scenarioSceneName)
                    || (hasOutGame && !IsLoaded(outGameSceneName))
                    || !IsLoaded(persistentSceneName)
                    || !selectionService.TryPrepareBattleSortie(definition, returnSceneName))
                {
                    ClearFailedSelection(pending, definition);
                    return ScenarioBattleSortieResult.PreparationFailed;
                }

                hasBegunSceneTransition = true;
                try
                {
                    bool success = await _transition.UnloadSourcesThenLoadSceneKeepingLoadingWithPersistentLifetimeAsync(
                        scenarioSceneName, hasOutGame ? outGameSceneName : null, persistentSceneName,
                        definition.TargetSceneName);
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
                _transitionInitializer.ShowScenarioBattleRecovery(
                    definition.TargetSceneName,
                    BuildOwnedScenes(scenarioSceneName, definition, hasOutGame ? outGameSceneName : null));
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

        private readonly SceneTransitionController _transition;
        private readonly ILoadingOperationExecutor _executor;
        private readonly SceneTransitionInitializer _transitionInitializer;

        /// <summary>
        ///     失敗した出撃の予約と選択を解除します。
        /// </summary>
        /// <param name="pending"> 予約済み遷移の状態。 </param>
        /// <param name="definition"> 出撃しようとしたバトルステージ定義。 </param>
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
        ///     失敗時の復旧で後片付けするシーンの一覧を作ります。
        /// </summary>
        /// <param name="scenarioSceneName"> シナリオシーン名。 </param>
        /// <param name="definition"> 出撃しようとしたバトルステージ定義。 </param>
        /// <param name="outGameSceneName"> OutGame シーン名。OutGame を経由していない場合はnull。 </param>
        /// <returns> 後片付けするシーン名の一覧。 </returns>
        private static string[] BuildOwnedScenes(
            string scenarioSceneName, BattleStageDefinition definition, string outGameSceneName)
        {
            List<string> scenes = new()
            {
                scenarioSceneName, definition.BattleSceneName, definition.TargetSceneName
            };
            if (!string.IsNullOrWhiteSpace(outGameSceneName)) { scenes.Add(outGameSceneName); }
            return scenes.ToArray();
        }

        /// <summary>
        ///     シーン名がすべて設定済みで、互いに重複していないか判定します。
        /// </summary>
        /// <param name="sceneNames"> 判定するシーン名。 </param>
        /// <returns> すべて有効で重複が無い場合はtrue。 </returns>
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
        ///     指定したシーンが読み込まれているか判定します。
        /// </summary>
        /// <param name="sceneName"> 判定するシーン名。 </param>
        /// <returns> 読み込まれている場合はtrue。 </returns>
        private static bool IsLoaded(string sceneName)
        {
            return !string.IsNullOrWhiteSpace(sceneName) && SceneManager.GetSceneByName(sceneName).isLoaded;
        }
    }
}
