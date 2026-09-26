using UnityEngine;

namespace KillChord.Runtime.Adaptor.OutGame.SkillTree
{
    /// <summary>
    ///     プレイヤーステータス画面のViewModel。
    /// </summary>
    public interface IPlayerStatusViewModel
    {
        /// <summary>
        ///     プレイヤーのステータス表示を更新する。
        /// </summary>
        public void Apply(PlayerStatusDTO dto);
    }
}
