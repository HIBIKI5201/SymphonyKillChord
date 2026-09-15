using KillChord.Runtime.Application.OutGame.Screen;
using KillChord.Runtime.Domain.OutGame.Screen;
using System;
using System.Collections.Generic;

namespace KillChord.Runtime.Adaptor.OutGame.Screen
{
    /// <summary>
    ///     制作メンバー一覧を DTO へ変換して View へ橋渡しする Presenter。
    /// </summary>
    public sealed class MemberListPresenter : IMemberListPresenter
    {
        /// <summary>
        ///     Presenter を初期化します。
        /// </summary>
        /// <param name="memberListApplicable"> 制作メンバー一覧の反映先です。 </param>
        /// <exception cref="ArgumentNullException"> 引数が null の場合に発生します。 </exception>
        public MemberListPresenter(IMemberListApplicable memberListApplicable)
        {
            _memberListApplicable = memberListApplicable ?? throw new ArgumentNullException(nameof(memberListApplicable));
        }

        /// <summary>
        ///     制作メンバー一覧を DTO へ変換して出力します。
        /// </summary>
        /// <param name="members"> 出力する制作メンバー情報の一覧です。 </param>
        public void Present(IReadOnlyList<MemberData> members)
        {
            if (members == null)
            {
                _memberListApplicable.ApplyMemberList(Array.Empty<MemberViewDTO>());
                return;
            }

            var memberViewDTOs = new MemberViewDTO[members.Count];
            for (int i = 0; i < members.Count; i++)
            {
                MemberData member = members[i];
                memberViewDTOs[i] = new MemberViewDTO(
                    member.Name.Value,
                    member.Class.Value,
                    member.Affiliation.Value);
            }

            _memberListApplicable.ApplyMemberList(memberViewDTOs);
        }

        private readonly IMemberListApplicable _memberListApplicable;
    }
}
