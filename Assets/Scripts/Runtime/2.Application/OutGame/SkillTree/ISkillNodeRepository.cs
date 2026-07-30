using KillChord.Runtime.Domain.OutGame.SkillTree;
using System.Collections.Generic;

namespace KillChord.Runtime.Application.OutGame.SkillTree
{
    /// <summary>
    ///     スキルノード定義を取得するRepositoryです。
    /// </summary>
    public interface ISkillNodeRepository
    {
        /// <summary>
        ///     全てのスキルノード定義を取得します。
        /// </summary>
        /// <returns> スキルノード定義一覧です。 </returns>
        public IReadOnlyCollection<SkillNodeEntity> GetAll();
    }
}
