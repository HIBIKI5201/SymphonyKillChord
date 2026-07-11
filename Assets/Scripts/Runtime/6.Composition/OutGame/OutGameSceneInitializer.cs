using KillChord.Runtime.Composition.OutGame.Bootstrap;
using KillChord.Runtime.Utility.Constant;
using KillChord.Runtime.View.OutGame.Screen;
using SymphonyFrameWork.System.SceneLoad;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KillChord.Runtime.Composition.OutGame
{
    /// <summary>
     ///     アウトゲーム共通 Signal を生成して公開するクラスです。
    /// </summary>
    [DefaultExecutionOrder(-100)]
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
            _isOwner = ServiceLocator.RegisterInstance(_outGameUiEvent, LocateType.Locator);
            return _isOwner;
        }

        /// <summary>
        ///     登録順の逆順でモジュールを終了します。
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

            return FindObjectsByType<OutGameInitializationModuleBase>(FindObjectsSortMode.None)
                .Where(module => module.isActiveAndEnabled && module.gameObject.scene == currentScene)
                .Cast<IOutGameInitializationModule>()
                .OrderBy(module => module.Order)
                .ToList();
        }

        private readonly OutGameInitializationCoordinator _initializationCoordinator = new();
        private List<IOutGameInitializationModule> _modules;
        private OutGameUIEvent _outGameUiEvent;
        private bool _isOwner;
    }
}
