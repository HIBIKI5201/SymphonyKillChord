using KillChord.Runtime.Domain.OutGame.SkillTree;

namespace KillChord.Runtime.Application.OutGame.SkillTree
{
    /// <summary>
    ///     製品固有の事情で解放させないスキルノードを判定するポリシーです。
    ///     体験版で未実装のスキルを解放させない場合などに差し替えます。
    /// </summary>
    public interface ISkillNodeLockPolicy
    {
        /// <summary>
        ///     指定ノードの解放を禁止するか判定します。
        /// </summary>
        /// <param name="nodeId"> 判定するノードIDです。 </param>
        /// <returns> 解放を禁止する場合はtrueです。 </returns>
        bool IsLocked(SkillNodeId nodeId);
    }
}
