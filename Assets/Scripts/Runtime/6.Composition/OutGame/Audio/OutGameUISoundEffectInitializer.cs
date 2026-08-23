using KillChord.Runtime.Adaptor.OutGame.Audio;
using KillChord.Runtime.Adaptor.Persistent.Music;
using KillChord.Runtime.Composition.OutGame.Bootstrap;
using KillChord.Runtime.Composition.Persistent.Music;
using KillChord.Runtime.InfraStructure.Addressables;
using KillChord.Runtime.Utility.Identity;
using KillChord.Runtime.View.OutGame.Audio;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace KillChord.Runtime.Composition.OutGame.Audio
{
    /// <summary>
    ///     SceneのUIDocumentへUI操作音のイベント監視を接続する。
    /// </summary>
    public sealed class OutGameUISoundEffectInitializer : OutGameInitializationModuleBase
    {
        /// <summary> モジュール名。 </summary>
        public override string ModuleName => nameof(OutGameUISoundEffectInitializer);

        /// <summary> 実行順。 </summary>
        public override int Order => 125;

        [SerializeField, Tooltip("UI操作音のイベントを監視するUIDocument。")]
        private UIDocument _uiDocument;

        [FormerlySerializedAs("_uiSoundEffectConfigAddress")]
        [SerializeField, SourceDataAddress]
        [Tooltip("UI操作と再生Cueの対応設定のAddressablesキー。")]
        private string _uiSoundEffectConfigKey;

        private IPlayableAudioSource _player;
        private IUISoundEffectCommand _command;
        private OutGameUISoundEffectModuleContainer _moduleContainer;
        private UIToolkitSoundEffectBinder _binder;
        private UISoundEffectConfig _config;
        private bool _isBuilt;

        /// <summary>
        ///     AddressablesからUI操作音設定をロードする。
        /// </summary>
        /// <param name="cancellationToken"> キャンセルトークン。 </param>
        /// <returns> ロードに成功した場合はtrue。 </returns>
        public override async Awaitable<bool> ResourceLoadAsync(CancellationToken cancellationToken)
        {
            _config = null;
            _uiSoundEffectConfigKey.ReleaseLoadedAsset(this);

            try
            {
                _config = await _uiSoundEffectConfigKey
                    .LoadAssetAsync<UISoundEffectConfig>(this, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _uiSoundEffectConfigKey.ReleaseLoadedAsset(this);
                throw;
            }
            catch (Exception exception)
            {
                _uiSoundEffectConfigKey.ReleaseLoadedAsset(this);
                Debug.LogError(
                    $"[{nameof(OutGameUISoundEffectInitializer)}] " +
                    $"{nameof(UISoundEffectConfig)}のロードに失敗しました。{exception.Message}",
                    this);
                return false;
            }

            if (_config != null)
            {
                return true;
            }

            _uiSoundEffectConfigKey.ReleaseLoadedAsset(this);
            Debug.LogError(
                $"[{nameof(OutGameUISoundEffectInitializer)}] " +
                $"{nameof(UISoundEffectConfig)}をロードできませんでした。" +
                $"Addressablesキー: {_uiSoundEffectConfigKey}",
                this);
            return false;
        }

        /// <summary>
        ///     UIDocument、Config、UI操作音の再生ポートを解決する。
        /// </summary>
        /// <returns> 必須構成を解決できた場合はtrue。 </returns>
        public override bool Build()
        {
            _isBuilt = false;
            _player = null;
            _command = null;
            _moduleContainer = null;

            if (_uiDocument == null)
            {
                Debug.LogError(
                    $"[{nameof(OutGameUISoundEffectInitializer)}] {nameof(UIDocument)}が設定されていません。",
                    this);
                return false;
            }

            if (_config == null)
            {
                Debug.LogError(
                    $"[{nameof(OutGameUISoundEffectInitializer)}] " +
                    $"{nameof(UISoundEffectConfig)}のロードが完了していません。",
                    this);
                return false;
            }

            if (!_config.Validate(out string errorMessage))
            {
                Debug.LogError(
                    $"[{nameof(OutGameUISoundEffectInitializer)}] {errorMessage}",
                    this);
                return false;
            }

            if (_uiDocument.rootVisualElement == null)
            {
                Debug.LogError(
                    $"[{nameof(OutGameUISoundEffectInitializer)}] rootVisualElementを取得できませんでした。",
                    this);
                return false;
            }

            if (!ServiceLocator.TryGetInstance(out UISoundEffectModuleContainer moduleContainer)
                || moduleContainer?.Player == null)
            {
                Debug.LogError(
                    $"[{nameof(OutGameUISoundEffectInitializer)}] " +
                    $"{nameof(UISoundEffectModuleContainer)}を取得できませんでした。",
                    this);
                return false;
            }

            _player = moduleContainer.Player;
            _command = new UISoundEffectCommand(_config, _player);

            if (ServiceLocator.TryGetInstance(out OutGameUISoundEffectModuleContainer _))
            {
                ServiceLocator.UnregisterInstance<OutGameUISoundEffectModuleContainer>();
            }

            _moduleContainer = new OutGameUISoundEffectModuleContainer(_command);
            if (!ServiceLocator.RegisterInstance(_moduleContainer))
            {
                Debug.LogError(
                    $"[{nameof(OutGameUISoundEffectInitializer)}] " +
                    $"{nameof(OutGameUISoundEffectModuleContainer)}を登録できませんでした。",
                    this);
                _moduleContainer = null;
                _command = null;
                _player = null;
                return false;
            }

            _isBuilt = true;
            return true;
        }

        /// <summary>
        ///     rootVisualElementへUI操作音のイベント監視を登録する。
        /// </summary>
        /// <returns> イベント監視を登録できた場合はtrue。 </returns>
        public override bool Ready()
        {
            if (!_isBuilt
                || _player == null
                || _command == null
                || _uiDocument?.rootVisualElement == null)
            {
                Debug.LogError(
                    $"[{nameof(OutGameUISoundEffectInitializer)}] Buildが完了していません。",
                    this);
                return false;
            }

            _binder?.Dispose();
            _binder = new UIToolkitSoundEffectBinder(
                _uiDocument.rootVisualElement,
                _config,
                _player);
            return true;
        }

        /// <summary>
        ///     UI操作音のイベント監視を解除する。
        /// </summary>
        public override void Shutdown()
        {
            _binder?.Dispose();
            _binder = null;

            if (ServiceLocator.TryGetInstance(
                    out OutGameUISoundEffectModuleContainer registeredContainer)
                && ReferenceEquals(registeredContainer, _moduleContainer))
            {
                ServiceLocator.UnregisterInstance<OutGameUISoundEffectModuleContainer>();
            }

            _uiSoundEffectConfigKey.ReleaseLoadedAsset(this);

            _config = null;
            _moduleContainer = null;
            _command = null;
            _player = null;
            _isBuilt = false;
        }
    }
}
