using KillChord.Runtime.Domain.OutGame.SkillTree;

namespace KillChord.Runtime.Composition.InGame.Player
{
    /// <summary>
    ///     InGameで使用するプレイヤーステータスボーナスを保持するContainerです。
    /// </summary>
    public sealed class PlayerStatusBonusModuleContainer
    {
        /// <summary>
        ///     Containerを生成します。
        /// </summary>
        /// <param name="playerStatusBonus"> プレイヤーステータスボーナスです。 </param>
        public PlayerStatusBonusModuleContainer(PlayerStatusBonus playerStatusBonus)
        {
            PlayerStatusBonus = playerStatusBonus;
        }

        /// <summary> プレイヤーステータスボーナスです。 </summary>
        public PlayerStatusBonus PlayerStatusBonus { get; }
    }
}
