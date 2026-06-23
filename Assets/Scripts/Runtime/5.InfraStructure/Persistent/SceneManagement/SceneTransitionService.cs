using KillChord.Runtime.Application.Persistent.Load;
using KillChord.Runtime.Application.Persistent.SceneManagement;
using KillChord.Runtime.Utility.Constant;
using SymphonyFrameWork.System.SceneLoad;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KillChord.Runtime.InfraStructure.Persistent.SceneManagement
{
    /// <summary>
    ///     シーン遷移サービスの実装。
    ///     シーンのロードとアンロードを管理し、シーン遷移を実現する。
    /// </summary>
    public class SceneTransitionService : ISceneTransitionService
    {
        public async Task<bool> ChangeSceneAsync(
            string fromSceneName,
            string toSceneName,
            IProgress<float> progress,
            CancellationToken cancellationToken = default)
        {
            progress?.Report(0f);

            if (string.IsNullOrEmpty(toSceneName))
            {
                Debug.LogError("シーン名が無効です。");
                return false;
            }

            if (!SceneLoader.GetExistScene(toSceneName, out Scene destinationScene)
                || !destinationScene.isLoaded)
            {
                var loadProgress = CreateProgressRange(
                    progress,
                    0f,
                    LoadingConstants.SCENE_LOAD_END_PROGRESS);

                bool loadSuccess = await SceneLoader.LoadScene(toSceneName,
                    CreateProgressCallback(loadProgress),
                    LoadSceneMode.Additive,
                    token: cancellationToken);

                if (!loadSuccess)
                {
                    Debug.LogError($"シーンのロードに失敗 : {toSceneName}");
                    return false;
                }
            }

            progress?.Report(LoadingConstants.SCENE_LOAD_END_PROGRESS);

            if (!SceneLoader.SetActiveScene(toSceneName))
            {
                Debug.LogError(
                    $"[{nameof(SceneTransitionService)}] " +
                    $"ActiveSceneの切り替えに失敗しました。" +
                    $" SceneName: {toSceneName}");

                return false;
            }

            if (!string.IsNullOrEmpty(fromSceneName) &&
                SceneLoader.GetExistScene(fromSceneName, out Scene sourceScene) && sourceScene.isLoaded)
            {
                var unloadProgress = CreateProgressRange(
                    progress,
                    LoadingConstants.SCENE_LOAD_END_PROGRESS,
                    1f);

                bool unloadSuccess = await SceneLoader.UnloadScene(
                    fromSceneName,
                    CreateProgressCallback(unloadProgress),
                    token: cancellationToken);
                if (!unloadSuccess)
                {
                    Debug.LogError($"シーンのアンロードに失敗 : {fromSceneName}");
                    return false;
                }
            }

            progress?.Report(1f);
            return true;
        }


        public async Task<bool> LoadAdditiveAsync(
            string sceneName,
            IProgress<float> progress,
            CancellationToken cancellationToken)
        {
            progress?.Report(0f);

            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogError("[SceneTransitionService] ロード対象のシーン名が空です。");
                return false;
            }

            if (SceneLoader.GetExistScene(sceneName, out Scene loadedScene) && loadedScene.isLoaded)
            {
                progress?.Report(1f);
                return true;
            }

            bool loadSuccess = await SceneLoader.LoadScene(
                sceneName,
                CreateProgressCallback(progress),
                LoadSceneMode.Additive,
                cancellationToken);

            if (!loadSuccess)
            {
                Debug.LogError($"[SceneTransitionService] シーンのAdditiveロードに失敗しました。SceneName: {sceneName}");
                return false;
            }

            progress?.Report(1f);
            return true;
        }


        public async Task<bool> UnloadAsync(
            string sceneName,
            IProgress<float> progress,
            CancellationToken cancellationToken)
        {
            progress?.Report(0f);

            if (string.IsNullOrWhiteSpace(sceneName))
            {
                return false;
            }

            if (!SceneLoader.GetExistScene(
                    sceneName,
                    out Scene loadedScene)
                || !loadedScene.isLoaded)
            {
                progress?.Report(1f);
                return true;
            }

            bool unloadSuccess = await SceneLoader.UnloadScene(
                   sceneName,
                   CreateProgressCallback(progress),
                   cancellationToken);

            if (!unloadSuccess)
            {
                Debug.LogError(
                    $"[{nameof(SceneTransitionService)}] " +
                    $"シーンのアンロードに失敗しました。" +
                    $" SceneName: {sceneName}");

                return false;
            }

            progress?.Report(1f);
            return true;
        }

        public async Task<bool> LoadAdditiveAndSetActiveAsync(
            string toSceneName,
            IProgress<float> progress,
            CancellationToken cancellationToken)
        {
            progress?.Report(0f);

            if (string.IsNullOrEmpty(toSceneName))
            {
                Debug.LogError("シーン名が無効です。");
                return false;
            }

            if (!SceneLoader.GetExistScene(toSceneName, out Scene loadedScene) || !loadedScene.isLoaded)
            {
                var loadProgress = CreateProgressRange(
                    progress,
                    0f,
                    1f - LoadingConstants.ACTIVE_SCENE_PROGRESS);

                bool loadSuccess = await SceneLoader.LoadScene(
                    toSceneName,
                    CreateProgressCallback(loadProgress),
                    LoadSceneMode.Additive,
                    token: cancellationToken)
                    ;
                if (!loadSuccess)
                {
                    Debug.LogError($"シーンのロードに失敗 : {toSceneName}");
                    return false;
                }
            }

            progress?.Report(1f - LoadingConstants.ACTIVE_SCENE_PROGRESS);

            if (!SceneLoader.SetActiveScene(toSceneName))
            {
                Debug.LogError($"ActiveSceneの切り替えに失敗しました。SceneName:{toSceneName}");
                return false;
            }

            progress?.Report(1f);
            return true;
        }

        public async Task<bool> UnloadAndSetActiveAsync(
            string unloadSceneName,
            string activeSceneName,
            IProgress<float> progress,
            CancellationToken cancellationToken)
        {
            progress?.Report(0f);

            if (!SceneLoader.SetActiveScene(activeSceneName))
            {
                Debug.LogError($"ActiveSceneの復帰に失敗しました。SceneName:{activeSceneName}");
                return false;
            }

            progress?.Report(LoadingConstants.ACTIVE_SCENE_PROGRESS);

            if (!string.IsNullOrWhiteSpace(unloadSceneName)
                && !string.IsNullOrWhiteSpace(activeSceneName)
                && SceneLoader.GetExistScene(unloadSceneName, out Scene loadedScene)
                && loadedScene.isLoaded)
            {
                var unloadProgress = CreateProgressRange(
                    progress,
                    LoadingConstants.ACTIVE_SCENE_PROGRESS,
                    1f);

                bool unloadSuccess = await SceneLoader.UnloadScene(
                    unloadSceneName,
                    CreateProgressCallback(unloadProgress),
                    token: cancellationToken);

                if (!unloadSuccess)
                {
                    Debug.LogError($"シーンのUnloadに失敗しました。SceneName:{unloadSceneName}");
                    return false;
                }
            }

            progress?.Report(1f);
            return true;
        }

        /// <summary>
        ///     進捗範囲変換クラスを作成する。
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="startProgress"></param>
        /// <param name="endProgress"></param>
        /// <returns></returns>
        private static LoadingProgressRange CreateProgressRange(
            IProgress<float> progress,
            float startProgress,
            float endProgress)
        {
            if (progress == null)
            {
                return null;
            }

            return new LoadingProgressRange(
                progress,
                startProgress,
                endProgress);
        }


        /// <summary>
        ///     進捗通知先をSceneLoaderへ渡せるコールバックへ変換する。
        /// </summary>
        /// <param name="progress"> 進捗通知先。 </param>
        /// <returns> 進捗通知先が存在する場合は通知処理、存在しない場合はnull。 </returns>
        private static Action<float> CreateProgressCallback(
            IProgress<float> progress)
        {
            return progress == null
                ? null
                : progress.Report;
        }
    }
}
