using KillChord.Runtime.Adaptor.Persistent.Music;
using KillChord.Runtime.Composition.Persistent.Bootstrap;
using KillChord.Runtime.View.Persistent.Music;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition.Persistent.Music
{
    /// <summary>
    ///     PersistentのUI操作音Sourceを使用可能にしてContainerを公開する。
    /// </summary>
    public sealed class UISoundEffectInitializer : PersistentInitializationModuleBase
    {
        /// <summary> モジュール名。 </summary>
        public override string ModuleName => nameof(UISoundEffectInitializer);

        /// <summary> 実行順。 </summary>
        public override int Order => 36;

        [SerializeField, Tooltip("SceneをまたいでUI操作音を再生するSoundEffectSource。")]
        private SoundEffectSource _soundEffectSource;

        private IPlayableAudioSource _player;
        private UISoundEffectModuleContainer _moduleContainer;
        private bool _isBuilt;

        /// <summary>
        ///     UI操作音の再生ポートと公開Containerを生成する。
        /// </summary>
        /// <returns> 必須構成の生成と登録に成功した場合はtrue。 </returns>
        public override bool Build()
        {
            _isBuilt = false;

            if (_soundEffectSource == null)
            {
                Debug.LogError(
                    $"[{nameof(UISoundEffectInitializer)}] {nameof(SoundEffectSource)}が設定されていません。",
                    this);
                return false;
            }

            if (ReferenceEquals(_soundEffectSource.gameObject, gameObject))
            {
                Debug.LogError(
                    $"[{nameof(UISoundEffectInitializer)}] UI操作音Sourceは子GameObjectへ設定してください。",
                    this);
                return false;
            }

            if (_soundEffectSource.GetComponent("CriAtomSource") == null)
            {
                Debug.LogError(
                    $"[{nameof(UISoundEffectInitializer)}] UI操作音SourceにCriAtomSourceがありません。",
                    this);
                return false;
            }

            if (_soundEffectSource.gameObject.activeSelf)
            {
                _soundEffectSource.gameObject.SetActive(false);
            }

            _player = _soundEffectSource;
            _moduleContainer = new UISoundEffectModuleContainer(_player);
            if (!ServiceLocator.RegisterInstance(_moduleContainer))
            {
                Debug.LogError(
                    $"[{nameof(UISoundEffectInitializer)}] " +
                    $"{nameof(UISoundEffectModuleContainer)}を登録できませんでした。",
                    this);
                _player = null;
                _moduleContainer = null;
                return false;
            }

            _isBuilt = true;
            return true;
        }

        /// <summary>
        ///     UI操作音Sourceを有効化して保存済みSE音量へ接続する。
        /// </summary>
        /// <returns> UI操作音Sourceを使用可能にできた場合はtrue。 </returns>
        public override bool Ready()
        {
            if (!_isBuilt || _soundEffectSource == null)
            {
                Debug.LogError(
                    $"[{nameof(UISoundEffectInitializer)}] Buildが完了していません。",
                    this);
                return false;
            }

            _soundEffectSource.gameObject.SetActive(true);
            if (!_soundEffectSource.isActiveAndEnabled)
            {
                Debug.LogError(
                    $"[{nameof(UISoundEffectInitializer)}] UI操作音Sourceを有効化できませんでした。",
                    this);
                return false;
            }

            return true;
        }

        /// <summary>
        ///     UI操作音Sourceを無効化して公開Containerを解除する。
        /// </summary>
        public override void Shutdown()
        {
            if (_soundEffectSource != null && _soundEffectSource.gameObject.activeSelf)
            {
                _soundEffectSource.gameObject.SetActive(false);
            }

            if (ServiceLocator.TryGetInstance(out UISoundEffectModuleContainer registeredContainer)
                && ReferenceEquals(registeredContainer, _moduleContainer))
            {
                ServiceLocator.UnregisterInstance<UISoundEffectModuleContainer>();
            }

            _isBuilt = false;
            _moduleContainer = null;
            _player = null;
        }
    }
}
