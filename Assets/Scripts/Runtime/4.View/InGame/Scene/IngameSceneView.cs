using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KillChord.Runtime.Utility.Constant;
using SymphonyFrameWork.System.SceneLoad;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Scene
{
    /// <summary>
    /// いったんここにすべて実装。後でレイヤー分けする
    /// </summary>
    public class IngameSceneView : MonoBehaviour
    {
        private List<string> _loadedScenes = new();

        /// <summary>
        ///     シーンを追加ロードする。
        /// </summary>
        /// <param name="sceneName">ロードするシーン名。</param>
        /// <returns>ロードに成功したか。</returns>
        public async ValueTask<bool> LoadScene(string sceneName)
        {
            _loadedScenes.Add(sceneName);
            return await SceneLoader.LoadSceneAsync(
                sceneName,
                priority: ScenePriorityResolver.Resolve(sceneName));
        }

        /// <summary>
        ///     ロード済みのシーンをアンロードする。
        /// </summary>
        /// <param name="sceneName">アンロードするシーン名。</param>
        /// <returns>アンロードに成功したか。</returns>
        public async ValueTask<bool> UnloadScene(string sceneName)
        {
            if (_loadedScenes.Contains(sceneName))
            {
                _loadedScenes.Remove(sceneName);
                return await SceneLoader.UnloadSceneAsync(sceneName);
            }
            else
            {
                Debug.LogError($"[{nameof(IngameSceneView)}] Scene {sceneName} does not exist.", this);
                return false;
            }
        }

        /// <summary>
        ///     ロード済みのシーンをすべてアンロードする。
        /// </summary>
        public async ValueTask UnloadAllScenes()
        {
            await SceneLoader.UnloadScenesAsync(_loadedScenes.ToArray());
        }
    }
}
