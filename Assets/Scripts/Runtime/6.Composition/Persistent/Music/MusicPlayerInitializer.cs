using KillChord.Runtime.Composition.Persistent.Bootstrap;
using KillChord.Runtime.View.InGame.Music;
using KillChord.Runtime.View.Persistent.Music;
using SymphonyFrameWork.System.ServiceLocate;
using UnityEngine;

namespace KillChord.Runtime.Composition.Persistent.Music
{
    /// <summary>
    ///     音楽再生機能の初期化を行うクラス。
    /// </summary>
    [RequireComponent(typeof(MusicPlayer))]
    public sealed class MusicPlayerInitializer : PersistentInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(MusicPlayerInitializer);

        /// <summary> 実行順です。 </summary>
        public override int Order => 20;

        /// <summary>
        ///     起動時に音楽プレイヤーの初期化を行う。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Build()
        {
            _musicPlayer = GetComponent<MusicPlayer>();
            MusicViewModel musicViewModel = new MusicViewModel();
            _musicPlayer.Bind(musicViewModel);
            _musicPlayer.Initialize();
            ServiceLocator.RegisterInstance(_musicPlayer, LocateTypeEnum.Locator);
            return true;
        }

        /// <summary>
        ///     登録済み音楽プレイヤーを解除する。
        /// </summary>
        public override void Shutdown()
        {
            if (ServiceLocator.TryGetInstance(out MusicPlayer registeredMusicPlayer)
                && ReferenceEquals(registeredMusicPlayer, _musicPlayer))
            {
                ServiceLocator.UnregisterInstance<MusicPlayer>();
            }

            _musicPlayer = null;
        }

        private MusicPlayer _musicPlayer;
    }
}
