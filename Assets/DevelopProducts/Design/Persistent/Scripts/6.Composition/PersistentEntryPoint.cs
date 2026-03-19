using SymphonyFrameWork.System.SceneLoad;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DevelopProducts.Persistent.Composition
{
    /// <summary>
    ///     常駐シーン起動時のエントリーポイントとなるクラス。
    /// </summary>
    public class PersistentEntryPoint : MonoBehaviour
    {
        [SerializeField] private string _firstSceneName;

        private CancellationTokenSource _cancellationTokenSource;

        private async void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            if (!SceneLoader.GetExistScene(_firstSceneName, out _))
            {
                bool success = await SceneLoader.LoadScene(_firstSceneName,
                    null,
                    LoadSceneMode.Additive,
                    _cancellationTokenSource.Token
                    );

                if (!success)
                {
                    Debug.LogError($"初回ロードに失敗 : {_firstSceneName}");
                    return;
                }
            }

            SceneLoader.SetActiveScene(_firstSceneName);
        }

        private void OnDestroy()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }
    }
}
