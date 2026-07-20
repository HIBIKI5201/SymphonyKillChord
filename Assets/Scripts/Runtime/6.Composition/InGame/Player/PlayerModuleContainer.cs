using KillChord.Runtime.Adaptor.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.OutGame.SkillTree;
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
        /// <param name="playerEntity"> プレイヤーEntityです。 </param>
        /// <param name="playerStatusBonus"> プレイヤーステータスボーナスです。 </param>
        public PlayerModuleContainer(
            PlayerInitializer playerInitializer,
            PlayerView playerView,
            CharacterEntity playerEntity,
            PlayerStatusBonus playerStatusBonus)
        {
            PlayerInitializer = playerInitializer;
            PlayerView = playerView;
            PlayerEntity = playerEntity;
            PlayerStatusBonus = playerStatusBonus;
        }

        /// <summary> プレイヤー初期化クラスです。 </summary>
        public PlayerInitializer PlayerInitializer { get; }

        /// <summary> プレイヤーViewです。 </summary>
        public PlayerView PlayerView { get; }

        /// <summary> プレイヤーEntityです。 </summary>
        public CharacterEntity PlayerEntity { get; }

        /// <summary> プレイヤーステータスボーナスです。 </summary>
        public PlayerStatusBonus PlayerStatusBonus { get; }

        /// <summary> プレイヤー攻撃Controllerです。 </summary>
        public PlayerAttackController PlayerAttackController { get; private set; }

        /// <summary>
        ///     他モジュールへ公開する攻撃Controllerを設定します。
        /// </summary>
        /// <param name="playerAttackController"> 公開する攻撃Controllerです。 </param>
        public void SetPlayerAttackController(PlayerAttackController playerAttackController)
        {
            PlayerAttackController = playerAttackController;
        }
    }
}
