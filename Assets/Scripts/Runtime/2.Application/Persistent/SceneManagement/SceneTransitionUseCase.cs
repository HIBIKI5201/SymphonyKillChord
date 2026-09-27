using KillChord.Runtime.Application.Persistent.Load;
using KillChord.Runtime.Utility.Constant;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Application.Persistent.SceneManagement
{
    /// <summary>
    ///     ロード画面を伴うシーン遷移を管理するUsecase。
    /// </summary>
    public class SceneTransitionUseCase
    {
        /// <summary>
        ///     必要な依存関係を指定して生成する。
        /// </summary>
        /// <param name="sceneTransitionService"> シーン遷移サービス。 </param>
        /// <param name="loadingOperationExecutor"> ロード画面付き処理の実行機能。 </param>
        /// <param name="sceneInitializationReadiness"> シーン初期化の完了待機機能。 </param>
        public SceneTransitionUseCase
            (ISceneTransitionService sceneTransitionService,
            ILoadingOperationExecutor loadingOperationExecutor,
            ISceneInitializationReadiness sceneInitializationReadiness)
        {
            _service = sceneTransitionService
                ?? throw new ArgumentNullException(
                    nameof(sceneTransitionService));

            _executor = loadingOperationExecutor
                ?? throw new ArgumentNullException(
                    nameof(loadingOperationExecutor));

            _sceneInitializationReadiness = sceneInitializationReadiness
                ?? throw new ArgumentNullException(
                    nameof(sceneInitializationReadiness));
        }

        /// <summary>
        ///    単純なシーン遷移を行う。
        /// </summary>
        /// <param name="fromSceneName"> 遷移元のシーン名。 </param>
        /// <param name="toSceneName"> 遷移先のシーン名 </param>
        /// <param name="ct"> キャンセルトークン。 </param>
        /// <returns> シーン遷移の成否を示すタスク。 </returns>
        public Task<bool> ChangeSceneAsync(
            string fromSceneName,
            string toSceneName,
            CancellationToken ct)
        {
            return _executor.ExecuteAsync(
                progress => LoadSceneAndWaitForReadyAsync(
                    toSceneName,
                    () => _service.ChangeSceneAsync(
                        fromSceneName,
                        toSceneName,
                        progress,
                        ct),
                    fromSceneName,
                    ct),
                ct);
        }

        /// <summary>
        ///     初期化状態を破棄してシーンを再読み込みし、初期化完了後にロード画面を閉じます。
        /// </summary>
        /// <param name="sceneName"> 再読み込みするシーン名です。 </param>
        /// <param name="cancellationToken"> キャンセルトークンです。 </param>
        /// <returns> 再読み込みと初期化に成功した場合はtrueです。 </returns>
        public Task<bool> ReloadSceneAsync(string sceneName, CancellationToken cancellationToken)
        {
            return _executor.ExecuteAsync(
                progress =>
                {
                    _sceneInitializationReadiness.Clear(sceneName);
                    return LoadSceneAndWaitForReadyAsync(
                        sceneName,
                        () => _service.ReloadSceneAsync(sceneName, progress, cancellationToken),
                        null,
                        cancellationToken);
                },
                cancellationToken);
        }

        /// <summary>
        ///    シーン遷移を行うが、ロード画面を閉じずに進捗を保持する。
        ///    既にアクティブなロードセッションが存在する場合（例: シーン初期化中に続けて次のシーンへ
        ///    遷移する場合）は、新規セッションを開始せずそのセッションを引き継いで完了させる。
        /// </summary>
        /// <param name="fromSceneName"> 遷移元のシーン名。 </param>
        /// <param name="toSceneName"> 遷移先のシーン名。 </param>
        /// <param name="ct"> キャンセルトークン。 </param>
        /// <returns> シーン遷移の成否を示すタスク。 </returns>
        public Task<bool> ChangeSceneKeepLoadingAsync(
            string fromSceneName,
            string toSceneName,
            CancellationToken ct)
        {
            LoadingExecutionOptions options = _executor.IsSessionActive
                ? LoadingExecutionOptions.ContinueAndComplete(
                    0f,
                    LoadingConstants.IN_GAME_SCENE_LOAD_END_PROGRESS)
                : LoadingExecutionOptions.KeepOpen(
                    0f,
                    LoadingConstants.IN_GAME_SCENE_LOAD_END_PROGRESS);

            return _executor.ExecuteAsync(
                progress => LoadSceneAndWaitForReadyAsync(
                    toSceneName,
                    () => _service.ChangeSceneAsync(
                        fromSceneName,
                        toSceneName,
                        progress,
                        ct),
                    fromSceneName,
                    ct),
                options,
                ct);
        }

        /// <summary>
        ///   Additiveシーンを読み込む。
        /// </summary>
        /// <param name="sceneName"> 読み込むAdditiveシーン名。 </param>
        /// <param name="ct"> キャンセルトークン。 </param>
        /// <returns> シーン読み込みの成否を示すタスク。 </returns>
        public Task<bool> LoadAdditiveAsync(
            string sceneName,
            CancellationToken ct)
        {
            return _executor.ExecuteAsync(
                progress => LoadSceneAndWaitForReadyAsync(
                    sceneName,
                    () => _service.LoadAdditiveAsync(
                        sceneName,
                        progress,
                        ct),
                    null,
                    ct),
                ct);
        }

        /// <summary>
        ///     Additiveシーンをアンロードする。
        /// </summary>
        /// <param name="sceneName"> アンロードするAdditiveシーン名。 </param>
        /// <param name="ct"> キャンセルトークン。 </param>
        /// <returns> シーンアンロードの成否を示すタスク。 </returns>
        public Task<bool> UnloadAsync(
            string sceneName,
            CancellationToken ct)
        {
            return _executor.ExecuteAsync(
                async progress =>
                {
                    bool isSuccess = await _service.UnloadAsync(
                        sceneName,
                        progress,
                        ct);

                    if (isSuccess)
                    {
                        _sceneInitializationReadiness.Clear(sceneName);
                    }

                    return isSuccess;
                },
                ct);
        }

        /// <summary>
        ///     指定したシーンをアンロードして、指定したシーンをアクティブにする。
        /// </summary>
        /// <param name="unloadSceneName"> アンロードするシーン名。 </param>
        /// <param name="activeSceneName"> アクティブにするシーン名。 </param>
        /// <param name="ct"> キャンセルトークン。 </param>
        /// <returns> シーン遷移の成否を示すタスク。 </returns>
        public Task<bool> UnloadAndSetActiveAsync(
            string unloadSceneName,
            string activeSceneName,
            CancellationToken ct)
        {
            return _executor.ExecuteAsync(
                async progress =>
                {
                    bool isSuccess = await _service.UnloadAndSetActiveAsync(
                        unloadSceneName,
                        activeSceneName,
                        progress,
                        ct);

                    if (isSuccess
                        && !string.IsNullOrWhiteSpace(unloadSceneName)
                        && !string.Equals(
                            unloadSceneName,
                            activeSceneName,
                            StringComparison.Ordinal))
                    {
                        _sceneInitializationReadiness.Clear(unloadSceneName);
                    }

                    return isSuccess;
                },
                ct);
        }

        /// <summary>
        ///     Additiveシーンをアンロードしてから、
        ///     基盤シーンから指定したシーンへ遷移する。
        /// </summary>
        /// <param name="additiveSceneName"> 先にアンロードするAdditiveシーン名。 </param>
        /// <param name="fromSceneName"> 遷移元となる基盤シーン名。 </param>
        /// <param name="toSceneName"> 遷移先となるシーン名。 </param>
        /// <param name="cancellationToken"> キャンセルトークン。 </param>
        /// <returns> すべてのシーン遷移処理に成功した場合はtrue。 </returns>
        public Task<bool> UnloadThenChangeSceneAsync(
            string additiveSceneName,
            string fromSceneName,
            string toSceneName,
            CancellationToken cancellationToken)
        {
            return _executor.ExecuteAsync(
                async progress =>
                {
                    // 進捗の前半で追加シーンを破棄し、元のシーンをアクティブに戻す。
                    IProgress<float> additiveUnloadProgress =
                        new LoadingProgressRange(
                            progress,
                            0f,
                            LoadingConstants
                                .RESULT_BATTLE_SCENE_UNLOAD_END_PROGRESS);

                    bool additiveUnloadSuccess =
                        await _service.UnloadAndSetActiveAsync(
                            additiveSceneName,
                            fromSceneName,
                            additiveUnloadProgress,
                            cancellationToken);

                    if (!additiveUnloadSuccess)
                    {
                        return false;
                    }

                    // 破棄した追加シーンの初期化状態を消す。
                    _sceneInitializationReadiness.Clear(additiveSceneName);

                    // 進捗の後半で遷移先のシーンを読み込み、初期化の完了を待つ。
                    IProgress<float> changeSceneProgress =
                        new LoadingProgressRange(
                            progress,
                            LoadingConstants
                                .RESULT_BATTLE_SCENE_UNLOAD_END_PROGRESS,
                            1f);

                    return await LoadSceneAndWaitForReadyAsync(
                        toSceneName,
                        () => _service.ChangeSceneAsync(
                            fromSceneName,
                            toSceneName,
                            changeSceneProgress,
                            cancellationToken),
                        fromSceneName,
                        cancellationToken);
                },
                cancellationToken);
        }

        /// <summary>
        ///     Additiveシーンをアンロードしてから、
        ///     基盤シーンを再読み込みする。
        /// </summary>
        /// <param name="additiveSceneName"> 先にアンロードするAdditiveシーン名。 </param>
        /// <param name="reloadSceneName"> 再読み込みする基盤シーン名。 </param>
        /// <param name="cancellationToken"> キャンセルトークン。 </param>
        /// <returns> すべての処理に成功した場合はtrue。</returns>
        public Task<bool> UnloadThenReloadSceneAsync(
            string additiveSceneName,
            string reloadSceneName,
            CancellationToken cancellationToken)
        {
            LoadingExecutionOptions options =
                LoadingExecutionOptions.KeepOpen(
                    0f,
                    LoadingConstants
                        .IN_GAME_SCENE_LOAD_END_PROGRESS);

            return _executor.ExecuteAsync(
                async progress =>
                {
                    // 進捗の前半で追加シーンを破棄し、読み込み直すシーンをアクティブに戻す。
                    IProgress<float> unloadProgress =
                        new LoadingProgressRange(
                            progress,
                            0f,
                            LoadingConstants
                                .RESULT_BATTLE_SCENE_UNLOAD_END_PROGRESS);

                    bool unloadSuccess =
                        await _service.UnloadAndSetActiveAsync(
                            additiveSceneName,
                            reloadSceneName,
                            unloadProgress,
                            cancellationToken);

                    if (!unloadSuccess)
                    {
                        return false;
                    }

                    // 両方のシーンの初期化状態を消す。
                    _sceneInitializationReadiness.Clear(additiveSceneName);
                    _sceneInitializationReadiness.Clear(reloadSceneName);

                    // 進捗の後半でシーンを読み込み直し、初期化の完了を待つ。
                    IProgress<float> reloadProgress =
                        new LoadingProgressRange(
                            progress,
                            LoadingConstants
                                .RESULT_BATTLE_SCENE_UNLOAD_END_PROGRESS,
                            1f);

                    return await LoadSceneAndWaitForReadyAsync(
                        reloadSceneName,
                        () => _service.ReloadSceneAsync(
                            reloadSceneName,
                            reloadProgress,
                            cancellationToken),
                        null,
                        cancellationToken);
                },
                options,
                cancellationToken);
        }

        /// <summary>
        ///     シーンをロードし、ルート初期化の完了まで待機します。
        /// </summary>
        /// <param name="sceneName"> ロードするシーン名です。 </param>
        /// <param name="loadOperation"> シーンロード処理です。 </param>
        /// <param name="unloadedSceneName"> ロード成功時に追跡解除するシーン名です。 </param>
        /// <param name="cancellationToken"> キャンセルトークンです。 </param>
        /// <returns> ロードと初期化の両方に成功した場合はtrueです。 </returns>
        private async Task<bool> LoadSceneAndWaitForReadyAsync(
            string sceneName,
            Func<Task<bool>> loadOperation,
            string unloadedSceneName,
            CancellationToken cancellationToken)
        {
            if (loadOperation == null)
            {
                throw new ArgumentNullException(nameof(loadOperation));
            }

            // 読み込みの前から初期化の通知を待ち受ける。
            _sceneInitializationReadiness.BeginTracking(sceneName);

            // 読み込みに失敗した場合は、初期化の待ち受けを失敗として終える。
            bool loadSuccess;
            try
            {
                loadSuccess = await loadOperation();
            }
            catch
            {
                _sceneInitializationReadiness.Clear(sceneName);
                throw;
            }

            if (!loadSuccess)
            {
                _sceneInitializationReadiness.Complete(sceneName, false);
                return false;
            }

            // 遷移元のシーンが破棄された場合は、その初期化状態を消す。
            if (!string.IsNullOrWhiteSpace(unloadedSceneName)
                && !string.Equals(
                    unloadedSceneName,
                    sceneName,
                    StringComparison.Ordinal))
            {
                _sceneInitializationReadiness.Clear(unloadedSceneName);
            }

            // 読み込んだシーンの初期化が終わるまで待つ。
            return await _sceneInitializationReadiness.WaitForReadyAsync(
                sceneName,
                cancellationToken);
        }

        /// <summary>
        ///     常駐シーンを基盤に両旧シーンを終了し、InGame の初期化まで待ちます。
        /// </summary>
        public Task<bool> UnloadSourcesThenLoadSceneKeepLoadingAsync(
            string scenarioSceneName, string outGameSceneName, string persistentSceneName,
            string inGameSceneName, CancellationToken cancellationToken)
        {
            return _executor.ExecuteAsync(
                async progress =>
                {
                    if (!await _service.UnloadAndSetActiveAsync(
                            scenarioSceneName, persistentSceneName,
                            new LoadingProgressRange(progress, 0f,
                                LoadingConstants.SCENARIO_SORTIE_UNLOAD_END_PROGRESS),
                            cancellationToken)) { return false; }
                    _sceneInitializationReadiness.Clear(scenarioSceneName);

                    if (!await _service.UnloadAndSetActiveAsync(
                            outGameSceneName, persistentSceneName,
                            new LoadingProgressRange(progress,
                                LoadingConstants.SCENARIO_SORTIE_UNLOAD_END_PROGRESS,
                                LoadingConstants.SCENARIO_SORTIE_SOURCES_UNLOAD_END_PROGRESS),
                            cancellationToken)) { return false; }
                    _sceneInitializationReadiness.Clear(outGameSceneName);

                    // InGame の Start より先に両旧シーンを終了し、専用の開始待ちを不要にします。
                    _sceneInitializationReadiness.Clear(inGameSceneName);
                    return await LoadSceneAndWaitForReadyAsync(
                        inGameSceneName,
                        () => _service.LoadAdditiveAsync(
                            inGameSceneName,
                            new LoadingProgressRange(progress,
                                LoadingConstants.SCENARIO_SORTIE_SOURCES_UNLOAD_END_PROGRESS, 1f),
                            cancellationToken),
                        null, cancellationToken);
                },
                LoadingExecutionOptions.KeepOpen(0f, LoadingConstants.IN_GAME_SCENE_LOAD_END_PROGRESS),
                cancellationToken);
        }

        private readonly ISceneTransitionService _service;
        private readonly ILoadingOperationExecutor _executor;
        private readonly ISceneInitializationReadiness _sceneInitializationReadiness;
    }
}
