using KillChord.Runtime.Domain.InGame.Buff;
using KillChord.Runtime.Domain.InGame.Character;

namespace KillChord.Runtime.Domain.InGame.Battle
{
    /// <summary>
    ///     攻撃者を表すインターフェース。
    /// </summary>
    public interface IAttacker
    {
        /// <summary>
        ///     攻撃者側のバフシステムを取得する。
        /// </summary>
        public IBuffSystem BuffSystem { get; }
    }
}
