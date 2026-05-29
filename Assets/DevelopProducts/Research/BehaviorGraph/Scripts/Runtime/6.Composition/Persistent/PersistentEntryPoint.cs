using SymphonyFrameWork.Attribute;
using SymphonyFrameWork.System.SceneLoad;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DevelopProducts.BehaviorGraph.Runtime.Composition
{
    /// <summary>
    ///     常駐シーン起動時のエントリーポイントとなるクラス。
    /// </summary>
    public class PersistentEntryPoint : MonoBehaviour
    {
        [SerializeField] private bool _active = true;
        [SerializeField, SceneNameSelector] private string _firstSceneName;

        private CancellationTokenSource _cancellationTokenSource;

        private async void Start()
        {
            if (!_active) return;
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
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }
    }
}