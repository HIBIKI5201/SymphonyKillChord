using KillChord.Runtime.Composition.InGame.Player;
using KillChord.Runtime.Composition.Persistent.Input;
using KillChord.Runtime.View.Persistent.Input;
using KillChord.Runtime.View.Persistent.Music;
using SymphonyFrameWork.System.ServiceLocate;
using System.Threading;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Bootstrap
{
    /// <summary>
    ///     シーン依存サービスの待機と公開を行うモジュールです。
    /// </summary>
    public sealed class SceneDependencyInitializationModule : InGameInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(SceneDependencyInitializationModule);

        /// <summary> 実行順です。 </summary>
        public override int Order => 0;

        /// <summary>
        ///     シーン依存サービスの待機ロードを行います。
        /// </summary>
        /// <param name="cancellationToken"> キャンセルトークンです。 </param>
        /// <returns> 成功した場合はtrue。 </returns>
        public override async Awaitable<bool> ResourceLoadAsync(CancellationToken cancellationToken)
        {
            _stageSceneInstance = await ServiceLocator.GetInstanceAsync<IStageSceneInstance>();
            if (_stageSceneInstance == null)
            {
                Debug.LogError($"[{nameof(SceneDependencyInitializationModule)}] {nameof(IStageSceneInstance)} の取得に失敗しました。", this);
                return false;
            }

            int retryCount = 0;
            _musicPlayer = ServiceLocator.GetInstance<MusicPlayer>();
            while (_musicPlayer == null && retryCount < MAX_RETRY_COUNT)
            {
                await Awaitable.NextFrameAsync(cancellationToken);
                _musicPlayer = ServiceLocator.GetInstance<MusicPlayer>();
                retryCount++;
            }

            if (_musicPlayer == null)
            {
                Debug.LogError($"[{nameof(SceneDependencyInitializationModule)}] {nameof(MusicPlayer)} の取得に失敗しました。", this);
                return false;
            }

            return true;
        }

        /// <summary>
        ///     シーン依存サービスを解決して登録します。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Build()
        {
            InputComposition inputComposition = ServiceLocator.GetInstance<InputComposition>();
            if (inputComposition == null)
            {
                Debug.LogError($"[{nameof(SceneDependencyInitializationModule)}] {nameof(InputComposition)} の取得に失敗しました。", this);
                return false;
            }

            PlayerInitializer playerInitializer = ServiceLocator.GetInstance<PlayerInitializer>();
            if (playerInitializer == null)
            {
                Debug.LogError($"[{nameof(SceneDependencyInitializationModule)}] {nameof(PlayerInitializer)} の取得に失敗しました。", this);
                return false;
            }

            UnityEngine.Camera mainCamera = UnityEngine.Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError($"[{nameof(SceneDependencyInitializationModule)}] MainCamera が見つかりません。", this);
                return false;
            }

            _container = new SceneDependencyModuleContainer(
                _stageSceneInstance,
                _musicPlayer,
                inputComposition,
                playerInitializer,
                mainCamera);
            ServiceLocator.RegisterInstance(_container);
            _container.InputComposition.GetInputMapController.EnableOnly(InputMapNames.Common);
            _isRegistered = true;
            return true;
        }

        /// <summary>
        ///     登録済みContainerを解除します。
        /// </summary>
        public override void Shutdown()
        {
            if (!_isRegistered)
            {
                return;
            }

            ServiceLocator.UnregisterInstance<SceneDependencyModuleContainer>();
            _isRegistered = false;
            _container = null;
            _stageSceneInstance = null;
            _musicPlayer = null;
        }

        private const int MAX_RETRY_COUNT = 20;

        private SceneDependencyModuleContainer _container;
        private IStageSceneInstance _stageSceneInstance;
        private MusicPlayer _musicPlayer;
        private bool _isRegistered;
    }
}
