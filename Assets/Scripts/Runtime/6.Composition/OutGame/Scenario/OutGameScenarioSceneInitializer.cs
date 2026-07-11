using KillChord.Runtime.Composition.OutGame.Bootstrap;
using KillChord.Runtime.Utility.Constant;
using System;
using System.Collections.Generic;
using System.Linq;
using SymphonyFrameWork.System.SceneLoad;
using UnityEngine;

namespace KillChord.Runtime.Composition.OutGame.Scenario
{
    /// <summary>
    ///     シナリオシーン専用の初期化ライフサイクルを起動するクラスです。
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public sealed class OutGameScenarioSceneInitializer : OutGameInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(OutGameScenarioSceneInitializer);

        /// <summary> 実行順です。 </summary>
        public override int Order => 0;

        /// <summary>
        ///     現在のシナリオシーン優先度を登録します。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Init()
        {
            return SceneLoader.RegisterLoadedScene(
                gameObject.scene.name,
                ScenePriorityResolver.Resolve(gameObject.scene.name));
        }

        /// <summary>
        ///     シナリオシーン初期化ライフサイクルを開始します。
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
                    Debug.LogError($"[{nameof(OutGameScenarioSceneInitializer)}] シナリオシーン初期化に失敗しました。", this);
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
    }
}
