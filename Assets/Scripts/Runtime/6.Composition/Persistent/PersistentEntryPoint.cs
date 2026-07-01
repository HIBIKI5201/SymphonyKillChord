using KillChord.Runtime.Adaptor.Persistent.SceneManagement;
using KillChord.Runtime.Composition.Persistent.SceneManagement;
using SymphonyFrameWork.Attribute;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using System.Threading;
using UnityEngine;

namespace KillChord.Runtime.Composition.Persistent
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

            if (!ServiceLocator.TryGetInstance(
                out SceneTransitionController controller))
            {
                Debug.LogError(
                    $"[{nameof(PersistentEntryPoint)}] " +
                    $"{nameof(SceneTransitionController)}が取得できません。" +
                    $"{nameof(SceneTransitionInitializer)}の初期化順を確認してください。",
                    this);

                return;
            }

            try
            {
                bool success = await controller.LoadAdditiveAndSetActiveAsync(
                    _firstSceneName,
                    _cancellationTokenSource.Token
                    );

                if (!success)
                {
                    Debug.LogError($"初回ロードに失敗 : {_firstSceneName}", this);
                    return;
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }

        private void OnDestroy()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }
    }
}