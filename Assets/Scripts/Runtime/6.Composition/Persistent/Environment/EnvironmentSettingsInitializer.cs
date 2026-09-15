using KillChord.Runtime.Adaptor.Persistent.Environment;
using KillChord.Runtime.Application.Persistent.Savedata;
using KillChord.Runtime.Composition.Persistent.Bootstrap;
using KillChord.Runtime.Domain.Persistent.Savedata;
using KillChord.Runtime.InfraStructure.Persistent.Savedata;
using KillChord.Runtime.View.Persistent.Environment;
using SymphonyFrameWork.System.ServiceLocate;
using System.Threading;
using UnityEngine;

namespace KillChord.Runtime.Composition.Persistent.Environment
{
    /// <summary>
    ///     環境設定を読み込み、Persistentシーンへ公開するモジュール。
    /// </summary>
    public sealed class EnvironmentSettingsInitializer : PersistentInitializationModuleBase
    {
        /// <summary> モジュール名。 </summary>
        public override string ModuleName => nameof(EnvironmentSettingsInitializer);

        /// <summary> 実行順。 </summary>
        public override int Order => 37;

        /// <summary>
        ///     保存済みの環境設定を読み込む。
        /// </summary>
        public override async Awaitable<bool> ResourceLoadAsync(CancellationToken cancellationToken)
        {
            _environmentSettingsRepository = new EnvironmentSettingsRepository(_defaultAsset);
            _environmentSettingsService = new EnvironmentSettingsService(_environmentSettingsRepository);
            _loadedSettings = await _environmentSettingsService.LoadAsync(cancellationToken);
            return _loadedSettings != null;
        }

        /// <summary>
        ///     各適用先へ保存値を適用し、環境設定Containerを登録する。
        /// </summary>
        public override bool Build()
        {
            if (!ServiceLocator.TryGetInstance(out ResolutionApplier resolutionApplier)
                || !ServiceLocator.TryGetInstance(out QualityApplier qualityApplier)
                || !ServiceLocator.TryGetInstance(out BrightnessApplier brightnessApplier))
            {
                Debug.LogError(
                    $"[{nameof(EnvironmentSettingsInitializer)}] 環境設定の適用先を取得できませんでした。",
                    this);
                return false;
            }

            _environmentSettingsViewModel = new EnvironmentSettingsViewModel();
            _environmentSettingsPresenter = new EnvironmentSettingsPresenter(_environmentSettingsViewModel, qualityApplier);
            _environmentSettingsController = new EnvironmentSettingsController(
                _loadedSettings,
                _environmentSettingsService,
                _environmentSettingsPresenter,
                resolutionApplier,
                qualityApplier,
                brightnessApplier);
            _environmentSettingsPresenter.Push(_loadedSettings);
            _moduleContainer = new EnvironmentSettingsModuleContainer(
                _environmentSettingsViewModel,
                _environmentSettingsController);

            return ServiceLocator.RegisterInstance(_moduleContainer);
        }

        /// <summary>
        ///     環境設定Containerの登録を解除する。
        /// </summary>
        public override void Shutdown()
        {
            if (ServiceLocator.TryGetInstance(out EnvironmentSettingsModuleContainer registeredContainer)
                && ReferenceEquals(registeredContainer, _moduleContainer))
            {
                ServiceLocator.UnregisterInstance<EnvironmentSettingsModuleContainer>();
            }

            _environmentSettingsViewModel?.Dispose();
            _environmentSettingsService?.Dispose();
            _moduleContainer = null;
            _environmentSettingsController = null;
            _environmentSettingsPresenter = null;
            _environmentSettingsViewModel = null;
            _environmentSettingsService = null;
            _environmentSettingsRepository = null;
            _loadedSettings = null;
        }

        [SerializeField, Tooltip("セーブデータが存在しない初回起動時に適用する環境設定の既定値")]
        private EnvironmentSettingsDefaultAsset _defaultAsset;

        private IEnvironmentSettingsRepository _environmentSettingsRepository;
        private EnvironmentSettingsService _environmentSettingsService;
        private EnvironmentSettingsViewModel _environmentSettingsViewModel;
        private EnvironmentSettingsPresenter _environmentSettingsPresenter;
        private EnvironmentSettingsController _environmentSettingsController;
        private EnvironmentSettingsModuleContainer _moduleContainer;
        private EnvironmentSettingsData _loadedSettings;
    }
}
