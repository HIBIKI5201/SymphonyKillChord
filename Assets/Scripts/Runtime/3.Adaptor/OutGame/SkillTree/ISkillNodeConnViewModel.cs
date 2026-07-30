using UnityEngine;

namespace KillChord.Runtime.Adaptor.OutGame.SkillTree
{
    /// <summary>
    ///     スキルノード接続線のViewModel。
    /// </summary>
    public interface ISkillNodeConnViewModel
    {
        /// <summary>
        ///     接続線の見た目を「通過」にする。
        /// </summary>
        public void SetPassed();

        /// <summary>
        ///     接続線の見た目を「未通過」にする。
        /// </summary>
        public void SetNotPassed();
    }
}
