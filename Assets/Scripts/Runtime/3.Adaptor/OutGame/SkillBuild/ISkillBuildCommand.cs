using System;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor.OutGame.SkillBuild
{
    /// <summary>
    ///     スキル編成操作を Application 層へ伝えるコマンドインターフェースです。
    /// </summary>
    public interface ISkillBuildCommand
    {
        /// <summary>
        ///     指定されたスキル編成を保存します。
        /// </summary>
        /// <param name="skillIds"> 保存するスキル ID の配列。 </param>
        /// <returns> 保存が成功した場合は true、失敗した場合は false。 </returns>
        public Task<bool> SaveAsync(ReadOnlyMemory<int> skillIds);

        /// <summary>
        ///     指定したスキルのレベルを1上げ、改造Pを1消費します。
        /// </summary>
        /// <param name="skillId"> 対象スキルID。 </param>
        /// <returns> 改造Pが不足している等の理由で実行できなかった場合は false。 </returns>
        public Task<bool> LevelUpAsync(int skillId);
    }
}
