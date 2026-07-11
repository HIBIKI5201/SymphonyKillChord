using KillChord.Runtime.Adaptor.InGame.StageSelect;
using KillChord.Runtime.Adaptor.Persistent.Load;
using KillChord.Runtime.Application.Persistent.Load;
using KillChord.Runtime.Application.Persistent.SceneManagement;
using KillChord.Runtime.Composition.InGame.Player;
using KillChord.Runtime.Utility.Constant;
using SymphonyFrameWork.System.SceneLoad;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KillChord.Runtime.Composition.InGame.Bootstrap
{
    /// <summary>
    ///     インゲーム初期化ライフサイクルを実行するクラスです。
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public sealed class IngameComposition : MonoBehaviour
    {
        /// <summary>
        ///     シーンロードと初期化フェーズを開始します。
        /// </summary>
        private async void Start()
        {
            RegisterCurrentScenePriority();

            if (!TryResolveBootDependencies(
                out SelectedBattleStageState selectedBattleStageState,
                out ILoadingOperationExecutor operationExecutor,
                out ISceneTransitionService sceneTransitionService))
            {
                FailActiveLoadingSession();
                return;
            }

            if (!selectedBattleStageState.HasSelectedBattleStage)
            {
                Debug.LogError($"[{nameof(IngameComposition)}] バトルステージが選択されていません。", this);
                FailActiveLoadingSession();
                return;
            }

            LoadingExecutionOptions options = LoadingExecutionOptions.ContinueAndComplete(
                LoadingConstants.IN_GAME_SCENE_LOAD_END_PROGRESS,
                1f);

            try
            {
                bool success = await operationExecutor.ExecuteAsync(
                    async totalProgress =>
                    {
                        LoadingProgressRange stageLoadProgress = new(
                            totalProgress,
                            0f,
                            LoadingConstants.STAGE_LOAD_END_PROGRESS);

                        bool loadSuccess = await sceneTransitionService.LoadAdditiveAsync(
                            selectedBattleStageState.BattleSceneName,
                            stageLoadProgress,
                            destroyCancellationToken);
                        if (!loadSuccess)
                        {
                            Debug.LogError($"[{nameof(IngameComposition)}] バトルシーンの読み込みに失敗しました。", this);
                            return false;
                        }

                        if (!await WaitForStageSceneReadyAsync(
                                selectedBattleStageState.BattleSceneName,
                                destroyCancellationToken))
                        {
                            Debug.LogError($"[{nameof(IngameComposition)}] バトルシーンの初期化待機に失敗しました。", this);
                            return false;
                        }

                        _modules = CollectModules();
                        if (_modules.Count == 0)
                        {
                            Debug.LogWarning($"[{nameof(IngameComposition)}] 初期化モジュールが見つかりません。", this);
                            return true;
                        }

                        LoadingProgressRange initializeProgress = new(
                            totalProgress,
                            LoadingConstants.STAGE_LOAD_END_PROGRESS,
                            1f);

                        return await _initializationCoordinator.InitializeAsync(
                            _modules,
                            initializeProgress,
                            destroyCancellationToken);
                    },
                    options,
                    destroyCancellationToken);

                if (!success)
                {
                    FailActiveLoadingSession();
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                FailActiveLoadingSession();
                Debug.LogException(exception, this);
            }
        }

        /// <summary>
        ///     現在のインゲームシーン優先度を登録します。
        /// </summary>
        private void RegisterCurrentScenePriority()
        {
            bool isRegistered = SceneLoader.RegisterLoadedScene(
                gameObject.scene.name,
                ScenePriorityResolver.Resolve(gameObject.scene.name));
            if (!isRegistered)
            {
                Debug.LogWarning($"[{nameof(IngameComposition)}] シーン優先度登録に失敗しました。{gameObject.scene.name}", this);
            }
        }

        /// <summary>
        ///     登録順の逆順でモジュールを終了します。
        /// </summary>
        private void OnDestroy()
        {
            if (_modules == null)
            {
                return;
            }

            for (int i = _modules.Count - 1; i >= 0; i--)
            {
                _modules[i]?.Shutdown();
            }

            _modules = null;
        }

        /// <summary>
        ///     シーン内の初期化モジュールを収集して実行順に並べます。
        /// </summary>
        /// <returns> 実行対象モジュール一覧です。 </returns>
        private List<IInGameInitializationModule> CollectModules()
        {
            return FindObjectsByType<InGameInitializationModuleBase>(FindObjectsSortMode.None)
                .Where(module => module.isActiveAndEnabled)
                .Cast<IInGameInitializationModule>()
                .OrderBy(module => module.Order)
                .ToList();
        }

        /// <summary>
        ///     起動に必要な常駐サービスを解決します。
        /// </summary>
        /// <param name="selectedBattleStageState"> 選択中ステージ状態です。 </param>
        /// <param name="operationExecutor"> ロード実行器です。 </param>
        /// <param name="sceneTransitionService"> シーン遷移サービスです。 </param>
        /// <returns> 解決に成功した場合はtrue。 </returns>
        private bool TryResolveBootDependencies(
            out SelectedBattleStageState selectedBattleStageState,
            out ILoadingOperationExecutor operationExecutor,
            out ISceneTransitionService sceneTransitionService)
        {
            selectedBattleStageState = null;
            operationExecutor = null;
            sceneTransitionService = null;

            if (!ServiceLocator.TryGetInstance(out selectedBattleStageState))
            {
                Debug.LogError($"[{nameof(IngameComposition)}] {nameof(SelectedBattleStageState)} が取得できませんでした。", this);
                return false;
            }

            if (!ServiceLocator.TryGetInstance(out operationExecutor))
            {
                Debug.LogError($"[{nameof(IngameComposition)}] {nameof(ILoadingOperationExecutor)} が取得できませんでした。", this);
                return false;
            }

            if (!ServiceLocator.TryGetInstance(out sceneTransitionService))
            {
                Debug.LogError($"[{nameof(IngameComposition)}] {nameof(ISceneTransitionService)} が取得できませんでした。", this);
                return false;
            }

            return true;
        }

        /// <summary>
        ///     実行中のロードセッションを失敗として終了します。
        /// </summary>
        private void FailActiveLoadingSession()
        {
            if (!ServiceLocator.TryGetInstance(out LoadingScreenController loadingScreenController))
            {
                return;
            }

            loadingScreenController.FailActiveSession();
        }

        /// <summary>
        ///     ステージシーンのロード完了とシーン参照登録完了を待機します。
        /// </summary>
        /// <param name="stageSceneName"> ステージシーン名です。 </param>
        /// <param name="cancellationToken"> キャンセルトークンです。 </param>
        /// <returns> 準備完了した場合はtrue。 </returns>
        private static async Awaitable<bool> WaitForStageSceneReadyAsync(
            string stageSceneName,
            System.Threading.CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(stageSceneName))
            {
                return false;
            }

            for (int retryCount = 0; retryCount < MAX_STAGE_READY_WAIT_FRAME; retryCount++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                Scene stageScene = SceneManager.GetSceneByName(stageSceneName);
                if (stageScene.isLoaded
                    && ServiceLocator.TryGetInstance(out IStageSceneInstance stageSceneInstance)
                    && stageSceneInstance != null
                    && stageSceneInstance.PlayerSpawnPointTransform != null)
                {
                    return true;
                }

                await Awaitable.NextFrameAsync(cancellationToken);
            }

            return false;
        }

        private const int MAX_STAGE_READY_WAIT_FRAME = 120;
        private readonly InGameInitializationCoordinator _initializationCoordinator = new();
        private List<IInGameInitializationModule> _modules;
    }
}
