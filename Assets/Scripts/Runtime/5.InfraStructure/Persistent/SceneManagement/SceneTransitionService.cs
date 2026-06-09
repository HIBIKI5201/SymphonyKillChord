using KillChord.Runtime.Application.Persistent.SceneManagement;
using SymphonyFrameWork.System.SceneLoad;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.Persistent.SceneManagement
{
    /// <summary>
    ///     シーン遷移サービスの実装。
    ///     シーンのロードとアンロードを管理し、シーン遷移を実現する。
    /// </summary>
    public class SceneTransitionService : ISceneTransitionService
    {
        public async ValueTask<bool> ChangeSceneAsync(
            string fromSceneName,
            string toSceneName,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(toSceneName))
            {
                Debug.LogError("シーン名が無効です。");
                return false;
            }

            if (!SceneLoader.GetExistScene(toSceneName, out _))
            {
                bool loadSuccess = await SceneLoader.LoadScene(toSceneName,
                    token: cancellationToken);
                if (!loadSuccess)
                {
                    Debug.LogError($"シーンのロードに失敗 : {toSceneName}");
                    return false;
                }
            }

            SceneLoader.SetActiveScene(toSceneName);

            if (!string.IsNullOrEmpty(fromSceneName) && SceneLoader.GetExistScene(fromSceneName, out _))
            {
                bool unloadSuccess = await SceneLoader.UnloadScene(fromSceneName,
                    token: cancellationToken);
                if (!unloadSuccess)
                {
                    Debug.LogError($"シーンのアンロードに失敗 : {fromSceneName}");
                    return false;
                }
            }
            return true;
        }

        public async ValueTask<bool> LoadAdditiveAndSetActiveAsync(
            string toSceneName, 
            CancellationToken cancellationToken)
        {
            if(string.IsNullOrEmpty(toSceneName))
            {
                Debug.LogError("シーン名が無効です。");
                return false;
            }

            if (!SceneLoader.GetExistScene(toSceneName, out _))
            {
                bool loadSuccess = await SceneLoader.LoadScene(
                    toSceneName,
                    token: cancellationToken)
                    ;
                if (!loadSuccess)
                {
                    Debug.LogError($"シーンのロードに失敗 : {toSceneName}");
                    return false;
                }
            }

            if (!SceneLoader.SetActiveScene(toSceneName))
            {
                Debug.LogError($"ActiveSceneの切り替えに失敗しました。SceneName:{toSceneName}");
                return false;
            }

            return true;
        }

        public async ValueTask<bool> UnloadAndSetActiveAsync(
            string unloadSceneName, string activeSceneName, 
            CancellationToken cancellationToken)
        {
            if (!SceneLoader.SetActiveScene(activeSceneName))
            {
                Debug.LogError($"ActiveSceneの復帰に失敗しました。SceneName:{activeSceneName}");
                return false;
            }

            if (!string.IsNullOrWhiteSpace(unloadSceneName) &&
                SceneLoader.GetExistScene(unloadSceneName, out _))
            {
                bool unloadSuccess = await SceneLoader.UnloadScene(
                    unloadSceneName,
                    token: cancellationToken);

                if (!unloadSuccess)
                {
                    Debug.LogError($"シーンのUnloadに失敗しました。SceneName:{unloadSceneName}");
                    return false;
                }
            }

            return true;
        }
    }
}
