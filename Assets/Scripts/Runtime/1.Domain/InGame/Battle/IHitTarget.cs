using KillChord.Runtime.Domain.InGame.Character;

namespace KillChord.Runtime.Domain.InGame.Battle
{
    /// <summary>
    ///     攻撃の対象を表すインターフェース。
    /// </summary>
    public interface IHitTarget
    {
        /// <summary> 体力情報。 </summary>
        HealthEntity Health { get; }
    }
}
