using System.Collections.Generic;

namespace KillChord.Runtime.Adaptor.OutGame.SkillTree
{
    /// <summary>
    ///     スキルツリー初期フォーカスのViewModelインターフェース。
    /// </summary>
    public interface ISkillTreeFocusViewModel
    {
        /// <summary>
        ///     初期フォーカス対象のノードIDを設定する。
        /// </summary>
        /// <param name="nodeIds"> 初期フォーカス対象のノードID。 </param>
        public void SetFocusTargets(IReadOnlyList<int> nodeIds);
    }
}
