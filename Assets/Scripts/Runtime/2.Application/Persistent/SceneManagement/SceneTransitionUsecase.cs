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
        /// <param name="sceneTransitionService">
        ///     シーン遷移サービス。
        /// </param>
        /// <param name="loadingOperationExecutor">
        ///     ロード画面付き処理の実行機能。
        /// </param>
        public SceneTransitionUsecase
            (ISceneTransitionService sceneTransitionService,
            ILoadingOperationExecutor loadingOperationExecutor)
        {
            _service = sceneTransitionService
                ?? throw new ArgumentNullException(
                    nameof(sceneTransitionService));

            _executor = loadingOperationExecutor
                ?? throw new ArgumentNullException(
                    nameof(loadingOperationExecutor));
        }

        public Task<bool> ChangeSceneAsync(
            string fromSceneName,
            string toSceneName,
            CancellationToken ct)
        {
            return _executor.ExecuteAsync(
                progress => _service.ChangeSceneAsync(
                    fromSceneName,
                    toSceneName,
                    progress,
                    ct),
                ct);
        }

        public Task<bool> ChangeSceneKeepLoadingAsync(
            string fromSceneName,
            string toSceneName,
            CancellationToken ct)
        {
            LoadingExecutionOptions options =
                LoadingExecutionOptions.KeepOpen(
                    0f,
                    LoadingConstants.IN_GAME_SCENE_LOAD_END_PROGRESS);

            return _executor.ExecuteAsync(
                progress => _service.ChangeSceneAsync(
                    fromSceneName,
                    toSceneName,
                    progress,
                    ct),
                options,
                ct);
        }

        public Task<bool> LoadAdditiveAsync(
            string scenrName,
            CancellationToken ct)
        {
            return _executor.ExecuteAsync(
                progress => _service.LoadAdditiveAsync(
                    scenrName,
                    progress,
                    ct),
                ct);
        }

        public Task<bool> UnloadAsync(
            string sceneName,
            CancellationToken ct)
        {
            return _executor.ExecuteAsync(
                progress => _service.UnloadAsync(
                    sceneName,
                    progress,
                    ct),
                ct);
        }

        public Task<bool> LoadAdditiveAndSetActiveAsync(
            string toSceneName,
            CancellationToken ct)
        {
            return _executor.ExecuteAsync(
                progress => _service.LoadAdditiveAndSetActiveAsync(
                        toSceneName,
                        progress,
                        ct),
                    ct);
        }

        public Task<bool> UnloadAndSetActiveAsync(
            string unloadSceneName,
            string activeSceneName,
            CancellationToken ct)
        {
            return _executor.ExecuteAsync(
                progress => _service.UnloadAndSetActiveAsync(
                        unloadSceneName,
                        activeSceneName,
                        progress,
                        ct),
                    ct);
        }

        /// <summary>
        ///     Additiveシーンをアンロードしてから、
        ///     基盤シーンから指定したシーンへ遷移する。
        /// </summary>
        /// <param name="additiveSceneName">
        ///     先にアンロードするAdditiveシーン名。
        /// </param>
        /// <param name="fromSceneName">
        ///     遷移元となる基盤シーン名。
        /// </param>
        /// <param name="toSceneName">
        ///     遷移先となるシーン名。
        /// </param>
        /// <param name="cancellationToken">
        ///     キャンセルトークン。
        /// </param>
        /// <returns>
        ///     すべてのシーン遷移処理に成功した場合はtrue。
        /// </returns>
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
                        await _service.UnloadAsync(
                            additiveSceneName,
                            additiveUnloadProgress,
                            cancellationToken);

                    if (!additiveUnloadSuccess)
                    {
                        return false;
                    }

                    IProgress<float> changeSceneProgress =
                        new LoadingProgressRange(
                            progress,
                            LoadingConstants
                                .RESULT_BATTLE_SCENE_UNLOAD_END_PROGRESS,
                            1f);

                    return await _service.ChangeSceneAsync(
                        fromSceneName,
                        toSceneName,
                        changeSceneProgress,
                        cancellationToken);
                },
                cancellationToken);
        }

        /// <summary>
        ///     Additiveシーンをアンロードしてから、
        ///     基盤シーンを再読み込みする。
        /// </summary>
        /// <param name="additiveSceneName">
        ///     先にアンロードするAdditiveシーン名。
        /// </param>
        /// <param name="reloadSceneName">
        ///     再読み込みする基盤シーン名。
        /// </param>
        /// <param name="cancellationToken">
        ///     キャンセルトークン。
        /// </param>
        /// <returns>すべての処理に成功した場合はtrue。</returns>
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
                        await _service.UnloadAsync(
                            additiveSceneName,
                            unloadProgress,
                            cancellationToken);

                    if (!unloadSuccess)
                    {
                        return false;
                    }

                    IProgress<float> reloadProgress =
                        new LoadingProgressRange(
                            progress,
                            LoadingConstants
                                .RESULT_BATTLE_SCENE_UNLOAD_END_PROGRESS,
                            1f);

                    return await _service.ReloadSceneAsync(
                        reloadSceneName,
                        reloadProgress,
                        cancellationToken);
                },
                options,
                cancellationToken);
        }

        private readonly ISceneTransitionService _service;
        private readonly ILoadingOperationExecutor _executor;
    }
}
