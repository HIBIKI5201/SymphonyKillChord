using SymphonyFrameWork.System.SceneLoad;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DevelopProducts.Persistent.Adaptor
{
    /// <summary>
    ///     シーン遷移サービスの実装。
    ///     シーンのロードとアンロードを管理し、シーン遷移を実現する。
    /// </summary>
    public class SceneTransitionService : ISceneTransitionService
    {
        public async Task<bool> ChangeSceneAsync(string fromSceneName,
            string toSceneName,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(toSceneName))
            {
                Debug.LogError("シーン名が無効です。");
                return false;
            }

            if (!SceneLoader.GetExistScene(toSceneName, out _))
            {
                bool loadSuccess = await SceneLoader.LoadScene(toSceneName,
                    null,
                    LoadSceneMode.Additive,
                    cancellationToken);
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
                    null,
                    cancellationToken);
                if (!unloadSuccess)
                {
                    Debug.LogError($"シーンのアンロードに失敗 : {fromSceneName}");
                    return false;
                }
            }
            return true;
        }
    }
}