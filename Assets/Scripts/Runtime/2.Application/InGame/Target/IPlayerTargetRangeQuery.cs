using KillChord.Runtime.Domain.InGame.Character;

namespace KillChord.Runtime.Application.InGame.Target
{
    /// <summary>
    ///     プレイヤーのターゲットが指定された範囲内にいるかどうかを判定するインターフェース。
    /// </summary>
    public interface IPlayerTargetRangeQuery
    {
        /// <summary>
        ///     指定されたターゲットが指定された範囲内にいるかどうかを判定します。
        /// </summary>
        /// <param name="target"> 判定対象。 </param>
        /// <param name="range"> 判定する範囲。 </param>
        /// <returns> 指定されたターゲットが指定された範囲内にいる場合はtrue、それ以外の場合はfalse。 </returns>
        bool IsWithinRange(CharacterEntity target, float range);
    }
}
