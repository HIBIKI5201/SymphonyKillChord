using KillChord.Runtime.Domain.InGame.Player;
using System.Collections.Generic;

namespace KillChord.Runtime.Adaptor
{
    /// <summary>
    ///     プレイヤーの行動制限を管理するクラス。
    ///     現段階ではスキルに対する制限のみ。
    /// </summary>
    public class PlayerActionRestrictionState
    {
        public PlayerActionRestrictionState()
        {
            _skillRestriction = new HashSet<PlayerActionRestrictionReason>();
        }

        /// <summary> スキル発動できるか。 </summary>
        public bool CanUseSkill => _skillRestriction != null && _skillRestriction.Count == 0;

        /// <summary>
        ///     理由を指定してプレイヤーのスキル発動を制限する。
        /// </summary>
        /// <param name="reason"></param>
        public void AddSkillRestriction(PlayerActionRestrictionReason reason)
        {
            _skillRestriction.Add(reason);
        }

        /// <summary>
        ///     理由を指定してプレイヤーのスキル発動制限を解除する。
        /// </summary>
        /// <param name="reason"></param>
        public void RemoveSkillRestriction(PlayerActionRestrictionReason reason)
        {
            _skillRestriction.Remove(reason);
        }

        /// <summary> スキル発動制限。<br/>複数理由で制限される状況を考慮してHashSetで管理する。 </summary>
        private readonly HashSet<PlayerActionRestrictionReason> _skillRestriction;
    }
}
