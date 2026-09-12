using System.Collections.Generic;

namespace KillChord.Runtime.Adaptor.OutGame.Screen
{
    /// <summary>
    ///     制作メンバー一覧を View へ反映できることを表す抽象。
    /// </summary>
    public interface IMemberListApplicable
    {
        /// <summary>
        ///     制作メンバー一覧を View へ反映します。
        /// </summary>
        /// <param name="members"> 反映する制作メンバー DTO の一覧です。 </param>
        void ApplyMemberList(IReadOnlyList<MemberViewDTO> members);
    }
}
