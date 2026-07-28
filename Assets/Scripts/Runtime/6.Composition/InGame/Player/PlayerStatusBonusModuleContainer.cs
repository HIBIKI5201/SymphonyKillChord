using KillChord.Runtime.Domain.OutGame.SkillTree;

namespace KillChord.Runtime.Composition.InGame.Player
{
    /// <summary>
    ///     InGame 開始時のプレイヤーステータスボーナスを PlayerInitializer へ公開する Container です。
    /// </summary>
    public sealed class PlayerStatusBonusModuleContainer
    {
        /// <summary>
        ///     Container を生成します。
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
