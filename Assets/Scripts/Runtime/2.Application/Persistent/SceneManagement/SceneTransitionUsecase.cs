using KillChord.Runtime.Application.Persistent.Load;
using KillChord.Runtime.Utility.Constant;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace KillChord.Runtime.Application.Persistent.SceneManagement
{
    public class SceneTransitionUsecase
    {
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
                progress =>_service.LoadAdditiveAndSetActiveAsync(
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
                progress =>_service.UnloadAndSetActiveAsync(
                        unloadSceneName,
                        activeSceneName,
                        progress,
                        ct),
                    ct);
        }

        private readonly ISceneTransitionService _service;
        private readonly ILoadingOperationExecutor _executor;
    }
}
