using KillChord.Runtime.Adaptor.Persistent.SceneManagement;
using KillChord.Runtime.Composition.Persistent.Bootstrap;
using KillChord.Runtime.Composition.Persistent.SceneManagement;
using SymphonyFrameWork.Attribute;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace KillChord.Runtime.Composition.Persistent
{
    /// <summary>
    ///     常駐シーン起動時のエントリーポイントとなるクラス。
    /// </summary>
    public sealed class PersistentEntryPoint : PersistentInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(PersistentEntryPoint);

        /// <summary> 実行順です。 </summary>
        public override int Order => 1000;

        [SerializeField] private bool _active = true;
        [SerializeField, SceneNameSelector] private string _firstSceneName;

        private CancellationTokenSource _cancellationTokenSource;
        private List<IPersistentInitializationModule> _modules;

        /// <summary>
        ///     常駐シーン初期化ライフサイクルを開始する。
        /// </summary>
        private async void Start()
        {
            _modules = CollectModules();
            if (_modules == null || _modules.Count == 0)
            {
                return;
            }

            try
            {
                bool isSuccess = await _initializationCoordinator.InitializeAsync(
                    _modules,
                    null,
                    destroyCancellationToken);
                if (!isSuccess)
                {
                    Debug.LogError($"[{nameof(PersistentEntryPoint)}] 常駐シーン初期化に失敗しました。", this);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }

        /// <summary>
        ///     初回シーンロードを開始する。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Ready()
        {
            if (!_active)
            {
                return true;
            }

            _cancellationTokenSource = new CancellationTokenSource();
            _ = LoadFirstSceneAsync();
            return true;
        }

        /// <summary>
        ///     登録済みモジュールの終了とトークン解放を行う。
        /// </summary>
        public override void Shutdown()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }

        /// <summary>
        ///     破棄時に登録済みモジュールを逆順で終了する。
        /// </summary>
        private void OnDestroy()
        {
            if (_modules != null)
            {
                for (int i = _modules.Count - 1; i >= 0; i--)
                {
                    _modules[i]?.Shutdown();
                }

                _modules = null;
            }
        }

        /// <summary>
        ///     初回シーンを加算ロードしてアクティブ化する。
        /// </summary>
        private async Awaitable LoadFirstSceneAsync()
        {
            if (!ServiceLocator.TryGetInstance(out SceneTransitionController controller))
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
                    _cancellationTokenSource.Token);

                if (!success)
                {
                    Debug.LogError($"初回ロードに失敗 : {_firstSceneName}", this);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }

        /// <summary>
        ///     シーン内の初期化モジュールを収集して実行順に並べます。
        /// </summary>
        /// <returns> 実行対象モジュール一覧です。 </returns>
        private List<IPersistentInitializationModule> CollectModules()
        {
            UnityEngine.SceneManagement.Scene currentScene = gameObject.scene;

            return FindObjectsByType<PersistentInitializationModuleBase>(FindObjectsSortMode.None)
                .Where(module => module.isActiveAndEnabled && module.gameObject.scene == currentScene)
                .Cast<IPersistentInitializationModule>()
                .OrderBy(module => module.Order)
                .ToList();
        }

        private readonly PersistentInitializationCoordinator _initializationCoordinator = new();
    }
}
