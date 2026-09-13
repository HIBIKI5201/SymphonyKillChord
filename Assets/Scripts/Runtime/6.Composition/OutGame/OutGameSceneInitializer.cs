using KillChord.Runtime.Application.Persistent.Load;
using KillChord.Runtime.Application.Persistent.SceneManagement;
using KillChord.Runtime.Composition.OutGame.Bootstrap;
using KillChord.Runtime.Utility.Collections;
using KillChord.Runtime.Utility.Constant;
using KillChord.Runtime.View.OutGame.Screen;
using SymphonyFrameWork.System.SceneLoad;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

namespace KillChord.Runtime.Composition.OutGame
{
    /// <summary>
    ///     アウトゲーム共通 Signal を生成して公開するクラスです。
    /// </summary>
    [DefaultExecutionOrder(ExecutionOrderConst.INITIALIZATION)]
    public sealed class OutGameSceneInitializer : OutGameInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(OutGameSceneInitializer);

        /// <summary> 実行順です。 </summary>
        public override int Order => 0;

        /// <summary>
        ///     現在のアウトゲームシーン優先度を登録します。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Init()
        {
            return SceneLoader.RegisterLoadedScene(
                gameObject.scene.name,
                ScenePriorityResolver.Resolve(gameObject.scene.name));
        }

        /// <summary>
        ///     アウトゲーム初期化ライフサイクルを開始します。
        /// </summary>
        private async void Start()
        {
            bool isSuccess = false;

            try
            {
                _modules = CollectModules();
                isSuccess = _modules == null
                    || _modules.Count == 0
                    || await _initializationCoordinator.InitializeAsync(
                        _modules,
                        null,
                        destroyCancellationToken);

                if (!isSuccess)
                {
                    Debug.LogError($"[{nameof(OutGameSceneInitializer)}] アウトゲーム初期化に失敗しました。", this);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
            finally
            {
                if (this != null)
                {
                    CompleteSceneInitialization(isSuccess);
                    if (!isSuccess && !destroyCancellationToken.IsCancellationRequested)
                    {
                        ShowInitializationFailure();
                    }
                }
            }
        }

        /// <summary>
        ///     OutGame 用 Signal を生成して登録します。
        /// </summary>
        public override bool Build()
        {
            if (ServiceLocator.TryGetInstance(out OutGameUIEvent _))
            {
                ServiceLocator.UnregisterInstance<OutGameUIEvent>();
            }

            _outGameUiEvent = new OutGameUIEvent();
            _isOwner = ServiceLocator.RegisterInstance(_outGameUiEvent, LocateTypeEnum.Locator);
            return _isOwner;
        }

        /// <summary>
        ///     登録順の逆順でモジュールを終了します。
        /// </summary>
        private void OnDestroy()
        {
            if (_failureView != null)
            {
                _failureView.OnRecoveryRequested -= RecoveryRequestedHandler;
            }

            if (_modules != null)
            {
                for (int i = _modules.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        _modules[i]?.Shutdown();
                    }
                    catch (Exception exception)
                    {
                        // 初期化途中のモジュールが失敗しても、残りの登録解除を続けます。
                        Debug.LogError(
                            $"[{nameof(OutGameSceneInitializer)}] {_modules[i]?.ModuleName} の終了処理に失敗しました。",
                            this);
                        Debug.LogException(exception, this);
                    }
                }

                _modules = null;
            }
        }

        /// <summary>
        ///     登録した Signal を解除します。
        /// </summary>
        public override void Shutdown()
        {
            if (_outGameUiEvent == null || !_isOwner)
            {
                return;
            }

            if (ServiceLocator.TryGetInstance(out OutGameUIEvent registeredOutGameUiEvent)
                && ReferenceEquals(registeredOutGameUiEvent, _outGameUiEvent))
            {
                ServiceLocator.UnregisterInstance<OutGameUIEvent>();
            }

            _outGameUiEvent = null;
            _isOwner = false;
        }

        /// <summary>
        ///     シーン内の初期化モジュールを収集して実行順に並べます。
        /// </summary>
        /// <returns> 実行対象モジュール一覧です。 </returns>
        private List<IOutGameInitializationModule> CollectModules()
        {
            UnityEngine.SceneManagement.Scene currentScene = gameObject.scene;
            OutGameInitializationModuleBase[] foundModules =
                FindObjectsByType<OutGameInitializationModuleBase>(FindObjectsSortMode.None);
            List<IOutGameInitializationModule> modules = new(foundModules.Length);

            for (int i = 0; i < foundModules.Length; i++)
            {
                OutGameInitializationModuleBase module = foundModules[i];
                if (!module.isActiveAndEnabled || module.gameObject.scene != currentScene)
                {
                    continue;
                }

                modules.Add(module);
            }

            modules.Sort(CompareModuleOrder);
            return modules;
        }

        /// <summary>
        ///     初期化モジュールの実行順を比較します。
        /// </summary>
        /// <param name="left"> 比較する左側のモジュールです。 </param>
        /// <param name="right"> 比較する右側のモジュールです。 </param>
        /// <returns> 実行順の比較結果です。 </returns>
        private static int CompareModuleOrder(
            IOutGameInitializationModule left,
            IOutGameInitializationModule right)
        {
            return left.Order.CompareTo(right.Order);
        }

        /// <summary>
        ///     現在のシーンの初期化結果を通知します。
        /// </summary>
        /// <param name="isSuccess"> 初期化に成功した場合はtrueです。 </param>
        private void CompleteSceneInitialization(bool isSuccess)
        {
            if (!ServiceLocator.TryGetInstance<ISceneInitializationReadiness>(out var readiness))
            {
                Debug.LogError(
                    $"[{nameof(OutGameSceneInitializer)}] " +
                    $"{nameof(ISceneInitializationReadiness)}が取得できません。",
                    this);
                return;
            }

            readiness.Complete(gameObject.scene.name, isSuccess);
        }

        /// <summary>
        ///     不完全な通常UIを無効にし、アセット不要の復帰画面を表示します。
        /// </summary>
        private void ShowInitializationFailure()
        {
            UIDocument[] documents = FindObjectsByType<UIDocument>(FindObjectsSortMode.None);
            foreach (UIDocument document in documents)
            {
                if (document.gameObject.scene == gameObject.scene)
                {
                    document.enabled = false;
                }
            }

            _failureView = gameObject.AddComponent<OutGameInitializationFailureView>();
            _failureView.Initialize(gameObject.scene.name == _titleSceneName ? "もう一度読み込む" : "タイトルへ戻る");
            _failureView.OnRecoveryRequested += RecoveryRequestedHandler;
        }

        /// <summary>
        ///     初期化に失敗したシーンからタイトルへ復帰します。タイトル自身の失敗時は再読み込みします。
        /// </summary>
        private async void RecoveryRequestedHandler()
        {
            if (_isRecovering)
            {
                return;
            }

            // 初期化失敗を受け取った元のロード処理が終了するまでは、次のロードを開始しません。
            if (ServiceLocator.TryGetInstance<ILoadingOperationExecutor>(out var executor)
                && executor.IsSessionActive)
            {
                return;
            }

            _isRecovering = true;
            _failureView.SetBusy(true);
            try
            {
                if (!ServiceLocator.TryGetInstance<SceneTransitionUsecase>(out var transition))
                {
                    Debug.LogError($"[{nameof(OutGameSceneInitializer)}] シーン遷移サービスが取得できませんでした。", this);
                    _failureView.ShowRecoveryFailed();
                    return;
                }

                string currentSceneName = gameObject.scene.name;
                // 復帰元の破棄後も、遷移先の初期化とロード画面の終了まで継続します。
                bool success = currentSceneName == _titleSceneName
                    ? await transition.ReloadSceneAsync(currentSceneName, CancellationToken.None)
                    : await transition.ChangeSceneAsync(currentSceneName, _titleSceneName, CancellationToken.None);

                if (!success && this != null)
                {
                    _failureView.ShowRecoveryFailed();
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                if (this != null)
                {
                    _failureView.ShowRecoveryFailed();
                }
            }
            finally
            {
                if (this != null)
                {
                    _isRecovering = false;
                    _failureView.SetBusy(false);
                }
            }
        }

        [SerializeField, Tooltip("初期化失敗時の復帰先となるタイトルシーン名です。")]
        private string _titleSceneName = "Title";

        private readonly OutGameInitializationCoordinator _initializationCoordinator = new();
        private List<IOutGameInitializationModule> _modules;
        private OutGameUIEvent _outGameUiEvent;
        private bool _isOwner;
        private OutGameInitializationFailureView _failureView;
        private bool _isRecovering;
    }
}
