using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SymphonyFrameWork.System.SceneLoad;
using UnityEngine;

namespace KillChord.Runtime.View
{
    /// <summary>
    /// いったんここにすべて実装。後でレイヤー分けする
    /// </summary>
    public class IngameSceneView : MonoBehaviour
    {
        private List<string> _loadedScenes = new();

        public ValueTask<bool> LoadScene(string sceneName)
        {
            _loadedScenes.Add(sceneName);
            return SceneLoader.LoadScene(sceneName);
        }

        public ValueTask<bool> UnloadScene(string sceneName)
        {
            if (_loadedScenes.Contains(sceneName))
            {
                _loadedScenes.Remove(sceneName);
                return SceneLoader.UnloadScene(sceneName);
            }
            else
            {
                Debug.LogError($"[IngameSceneView] Scene {sceneName} does not exist.]");
                return default;
            }
        }

        public async ValueTask UnloadAllScenes()
        {
            var unloadTasks = new Task<bool>[_loadedScenes.Count];
            for (int i = 0; i < _loadedScenes.Count; i++)
            {
                unloadTasks[i] = SceneLoader.UnloadScene(_loadedScenes[i]).AsTask();
            }

            await Task.WhenAll(unloadTasks);
        }
    }
}