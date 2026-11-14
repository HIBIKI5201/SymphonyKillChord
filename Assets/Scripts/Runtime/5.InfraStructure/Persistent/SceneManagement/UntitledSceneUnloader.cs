using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KillChord.Runtime.InfraStructure.Persistent.SceneManagement
{
    /// <summary>
    ///     エディタで保存していない空のシーン（Untitled）からPlayを始めたときに残る、そのシーンを破棄する。
    ///     名前の無いシーンはSceneLoaderの起動時リセットで破棄されず、
    ///     中のMain CameraがCamera.mainに選ばれたり、リトライ時の一時ActiveSceneに選ばれたりするため。
    /// </summary>
    public static class UntitledSceneUnloader
    {
        /// <summary>
        ///     読み込まれている名前の無いシーンをすべて破棄する。
        /// </summary>
        /// <param name="cancellationToken"> キャンセルトークン。 </param>
        /// <returns> 破棄したシーンの数。 </returns>
        public static async Task<int> UnloadAsync(CancellationToken cancellationToken)
        {
            List<Scene> untitledScenes = CollectUntitledScenes();

            // Unityは最後の1シーンを破棄できないため、ほかのシーンが無いときは何もしない。
            if (untitledScenes.Count == 0
                || SceneManager.sceneCount <= untitledScenes.Count)
            {
                return 0;
            }

            int unloadedCount = 0;
            foreach (Scene scene in untitledScenes)
            {
                cancellationToken.ThrowIfCancellationRequested();

                AsyncOperation operation = SceneManager.UnloadSceneAsync(scene);
                if (operation == null)
                {
                    Debug.LogWarning(
                        $"[{nameof(UntitledSceneUnloader)}] 名前の無いシーンを破棄できませんでした。");
                    continue;
                }

                // 破棄が終わるまで待ち、次のシーンへ進む。
                while (!operation.isDone)
                {
                    await Awaitable.NextFrameAsync(cancellationToken);
                }

                unloadedCount++;
            }

            return unloadedCount;
        }

        /// <summary>
        ///     読み込まれている名前の無いシーンを集める。
        /// </summary>
        /// <returns> 名前もパスも無いシーンの一覧。 </returns>
        private static List<Scene> CollectUntitledScenes()
        {
            List<Scene> scenes = new();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.IsValid()
                    && scene.isLoaded
                    && string.IsNullOrEmpty(scene.name)
                    && string.IsNullOrEmpty(scene.path))
                {
                    scenes.Add(scene);
                }
            }

            return scenes;
        }
    }
}
