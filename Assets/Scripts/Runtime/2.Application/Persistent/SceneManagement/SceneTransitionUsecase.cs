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
    public class SceneTransitionUsecase
    {
        /// <summary>
        ///     必要な依存関係を指定して生成する。
        /// </summary>
        /// <param name="sceneTransitionService"> シーン遷移サービス。 </param>
        /// <param name="loadingOperationExecutor"> ロード画面付き処理の実行機能。 </param>
        /// <param name="sceneInitializationReadiness"> シーン初期化の完了待機機能。 </param>
        public SceneTransitionUsecase
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

                    _sceneInitializationReadiness.Clear(additiveSceneName);

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

                    _sceneInitializationReadiness.Clear(additiveSceneName);
                    _sceneInitializationReadiness.Clear(reloadSceneName);

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

            _sceneInitializationReadiness.BeginTracking(sceneName);

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

            if (!string.IsNullOrWhiteSpace(unloadedSceneName)
                && !string.Equals(
                    unloadedSceneName,
                    sceneName,
                    StringComparison.Ordinal))
            {
                _sceneInitializationReadiness.Clear(unloadedSceneName);
            }

            return await _sceneInitializationReadiness.WaitForReadyAsync(
                sceneName,
                cancellationToken);
        }

        private readonly ISceneTransitionService _service;
        private readonly ILoadingOperationExecutor _executor;
        private readonly ISceneInitializationReadiness _sceneInitializationReadiness;
    }
}
