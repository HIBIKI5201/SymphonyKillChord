using UnityEngine;

namespace KillChord.Runtime.Adaptor.OutGame.SkillTree
{
    /// <summary>
    ///     プレイヤーステータス画面のViewModel。
    /// </summary>
    public interface IPlayerStatusViewModel
    {
        public void Apply(PlayerStatusDTO dto);
    }
}
