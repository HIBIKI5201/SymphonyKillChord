using KillChord.Runtime.Adaptor.Persistent.Music;
using KillChord.Runtime.Composition.OutGame.Bootstrap;
using KillChord.Runtime.Composition.Persistent.Music;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition.OutGame.Music
{
    /// <summary>
    ///     Sceneに設定されたBGM CueをPersistentのMusicPlayerへ通知する。
    /// </summary>
    public sealed class OutGameBgmInitializer : OutGameInitializationModuleBase
    {
        /// <summary> モジュール名。 </summary>
        public override string ModuleName => nameof(OutGameBgmInitializer);

        /// <summary> 実行順。 </summary>
        public override int Order => 30;

        [SerializeField, Tooltip("このSceneで再生するBGMのCue名。")]
        private string _cueName;

        private IBgmCuePlayer _cuePlayer;
        private bool _isBuilt;

        /// <summary>
        ///     BGM CueとMusicPlayerの必須構成を確認する。
        /// </summary>
        /// <returns> 必須構成を解決できた場合はtrue。 </returns>
        public override bool Build()
        {
            _isBuilt = false;
            _cuePlayer = null;

            if (string.IsNullOrWhiteSpace(_cueName))
            {
                Debug.LogError(
                    $"[{nameof(OutGameBgmInitializer)}] BGMのCue名が設定されていません。",
                    this);
                return false;
            }

            if (!ServiceLocator.TryGetInstance(out MusicPlayerModuleContainer moduleContainer)
                || moduleContainer?.CuePlayer == null)
            {
                Debug.LogError(
                    $"[{nameof(OutGameBgmInitializer)}] " +
                    $"{nameof(MusicPlayerModuleContainer)}を取得できませんでした。",
                    this);
                return false;
            }

            _cuePlayer = moduleContainer.CuePlayer;
            _isBuilt = true;
            return true;
        }

        /// <summary>
        ///     Sceneに設定されたBGM Cueへ切り替える。
        /// </summary>
        /// <returns> BGM切替を要求できた場合はtrue。 </returns>
        public override bool Ready()
        {
            if (!_isBuilt || _cuePlayer == null)
            {
                Debug.LogError(
                    $"[{nameof(OutGameBgmInitializer)}] Buildが完了していません。",
                    this);
                return false;
            }

            _cuePlayer.SetCue(_cueName);
            return true;
        }

        /// <summary>
        ///     Scene固有の参照を解放する。
        /// </summary>
        public override void Shutdown()
        {
            _isBuilt = false;
            _cuePlayer = null;
        }
    }
}
