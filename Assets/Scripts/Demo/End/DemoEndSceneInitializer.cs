using KillChord.Runtime.Application.Persistent.SceneManagement;
using KillChord.Runtime.Composition.OutGame.Bootstrap;
using KillChord.Runtime.Utility.Collections;
using KillChord.Runtime.Utility.Constant;
using SymphonyFrameWork.System.SceneLoad;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KillChord.Demo.End
{
    /// <summary>
    ///     体験版終了シーンの背景ステージ読み込みと初期化ライフサイクルを起動するクラスです。
    /// </summary>
    [DefaultExecutionOrder(ExecutionOrderConst.INITIALIZATION)]
    public sealed class DemoEndSceneInitializer : OutGameInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(DemoEndSceneInitializer);

        /// <summary> 実行順です。 </summary>
        public override int Order => 0;

        /// <summary>
        ///     現在の体験版終了シーンの優先度を登録します。
        /// </summary>
        /// <returns> 成功した場合はtrueです。 </returns>
        public override bool Init()
        {
            return SceneLoader.RegisterLoadedScene(
                gameObject.scene.name,
                ScenePriorityResolver.Resolve(gameObject.scene.name));
        }

        private const int MAX_BACKGROUND_WAIT_FRAME_COUNT = 600;

        [SerializeField, Tooltip("体験版終了演出の再生設定です。")]
        private DemoEndSequenceConfig _config;

        private readonly OutGameInitializationCoordinator _initializationCoordinator = new();
        private List<IOutGameInitializationModule> _modules;

        /// <summary>
        ///     背景ステージを読み込んでから初期化ライフサイクルを開始します。
        /// </summary>
        private async void Start()
        {
            if (!IsBootedThroughPersistentFlow)
            {
                Debug.Log(
                    $"[{nameof(DemoEndSceneInitializer)}] " +
                    $"常駐シーンが未起動のため、体験版終了シーンの初期化を行いません。{gameObject.scene.name}",
                    this);
                return;
            }

            bool isSuccess = false;

            try
            {
                if (_config == null)
                {
                    Debug.LogError(
                        $"[{nameof(DemoEndSceneInitializer)}] {nameof(_config)} が設定されていません。",
                        this);
                    return;
                }

                if (!await TryLoadBackgroundSceneAsync(destroyCancellationToken))
                {
                    return;
                }

                _modules = CollectModules();
                isSuccess = _modules == null
                    || _modules.Count == 0
                    || await _initializationCoordinator.InitializeAsync(
                        _modules,
                        null,
                        destroyCancellationToken);

                if (!isSuccess)
                {
                    Debug.LogError(
                        $"[{nameof(DemoEndSceneInitializer)}] 体験版終了シーンの初期化に失敗しました。",
                        this);
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
                CompleteSceneInitialization(isSuccess);
            }
        }

        /// <summary>
        ///     常駐シーンの初期化を経てこのシーンが起動されたかを示します。
        /// </summary>
        private static bool IsBootedThroughPersistentFlow =>
            ServiceLocator.IsExistInstance<ISceneInitializationReadiness>();

        /// <summary>
        ///     登録順の逆順でモジュールを終了します。
        /// </summary>
        private void OnDestroy()
        {
            if (_modules == null)
            {
                return;
            }

            for (int i = _modules.Count - 1; i >= 0; i--)
            {
                _modules[i]?.Shutdown();
            }

            _modules = null;
        }

        /// <summary>
        ///     背景となるステージシーンをAdditiveで読み込み、読み込み完了まで待機します。
        /// </summary>
        /// <param name="cancellationToken"> キャンセル用のトークンです。 </param>
        /// <returns> 読み込みに成功した場合はtrueです。 </returns>
        private async Awaitable<bool> TryLoadBackgroundSceneAsync(
            System.Threading.CancellationToken cancellationToken)
        {
            string backgroundSceneName = _config.BackgroundSceneName;
            if (string.IsNullOrWhiteSpace(backgroundSceneName))
            {
                Debug.LogError(
                    $"[{nameof(DemoEndSceneInitializer)}] 背景シーン名が設定されていません。",
                    this);
                return false;
            }

            if (!ServiceLocator.TryGetInstance(out ISceneTransitionService sceneTransitionService))
            {
                Debug.LogError(
                    $"[{nameof(DemoEndSceneInitializer)}] " +
                    $"{nameof(ISceneTransitionService)} が取得できません。",
                    this);
                return false;
            }

            bool loadSuccess = await sceneTransitionService.LoadAdditiveAsync(
                backgroundSceneName,
                null,
                cancellationToken);
            if (!loadSuccess)
            {
                Debug.LogError(
                    $"[{nameof(DemoEndSceneInitializer)}] 背景シーンの読み込みに失敗しました。" +
                    $" SceneName: {backgroundSceneName}",
                    this);
                return false;
            }

            // 背景シーン内のオブジェクトがAwakeを終えてからTimelineを開始させる。
            for (int waitFrameCount = 0;
                waitFrameCount < MAX_BACKGROUND_WAIT_FRAME_COUNT;
                waitFrameCount++)
            {
                Scene backgroundScene = SceneManager.GetSceneByName(backgroundSceneName);
                if (backgroundScene.IsValid() && backgroundScene.isLoaded)
                {
                    return true;
                }

                await Awaitable.NextFrameAsync(cancellationToken);
            }

            Debug.LogError(
                $"[{nameof(DemoEndSceneInitializer)}] 背景シーンの読み込み待機がタイムアウトしました。" +
                $" SceneName: {backgroundSceneName}",
                this);
            return false;
        }

        /// <summary>
        ///     シーン内の初期化モジュールを収集して実行順に並べます。
        /// </summary>
        /// <returns> 実行対象モジュール一覧です。 </returns>
        private List<IOutGameInitializationModule> CollectModules()
        {
            Scene currentScene = gameObject.scene;
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
                    $"[{nameof(DemoEndSceneInitializer)}] " +
                    $"{nameof(ISceneInitializationReadiness)} が取得できません。",
                    this);
                return;
            }

            readiness.Complete(gameObject.scene.name, isSuccess);
        }
    }
}
