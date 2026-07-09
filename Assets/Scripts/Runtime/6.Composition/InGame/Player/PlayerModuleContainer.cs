using KillChord.Runtime.View.InGame.Player;

namespace KillChord.Runtime.Composition.InGame.Player
{
    /// <summary>
    ///     プレイヤーモジュールの公開物を保持するContainerです。
    /// </summary>
    public sealed class PlayerModuleContainer
    {
        /// <summary>
        ///     Containerを生成します。
        /// </summary>
        /// <param name="playerInitializer"> プレイヤー初期化クラスです。 </param>
        /// <param name="playerView"> プレイヤーViewです。 </param>
        public PlayerModuleContainer(PlayerInitializer playerInitializer, PlayerView playerView)
        {
            PlayerInitializer = playerInitializer;
            PlayerView = playerView;
        }

        /// <summary> プレイヤー初期化クラスです。 </summary>
        public PlayerInitializer PlayerInitializer { get; }

        /// <summary> プレイヤーViewです。 </summary>
        public PlayerView PlayerView { get; }
    }
}
