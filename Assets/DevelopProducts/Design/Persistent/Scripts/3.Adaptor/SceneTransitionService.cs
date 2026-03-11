using SymphonyFrameWork.System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionService : ISceneTransitionService
{
    public async Task<bool> ChangeSceneAsync(string fromSceneName,
        string toSceneName,
        CancellationTokenSource cancellationTokenSource)
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
                cancellationTokenSource.Token);

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
                cancellationTokenSource.Token);
            if (!unloadSuccess)
            {
                Debug.LogError($"シーンのアンロードに失敗 : {fromSceneName}");
                return false;
            }
        }
        return true;
    }
}
