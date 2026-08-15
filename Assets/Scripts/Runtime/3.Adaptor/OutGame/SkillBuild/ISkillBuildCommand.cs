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
    }
}
