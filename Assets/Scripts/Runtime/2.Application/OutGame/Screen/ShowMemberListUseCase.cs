using KillChord.Runtime.Domain.OutGame.Screen;
using System;
using System.Collections.Generic;

namespace KillChord.Runtime.Application.OutGame.Screen
{
    /// <summary>
    ///     制作メンバー一覧を取得して出力するユースケース。
    /// </summary>
    public sealed class ShowMemberListUseCase
    {
        /// <summary>
        ///     ユースケースを初期化します。
        /// </summary>
        /// <param name="memberRepository"> 制作メンバー情報リポジトリです。 </param>
        /// <param name="memberListPresenter"> 制作メンバー一覧の出力先です。 </param>
        /// <exception cref="ArgumentNullException"> 引数が null の場合に発生します。 </exception>
        public ShowMemberListUseCase(IMemberRepository memberRepository, IMemberListPresenter memberListPresenter)
        {
            _memberRepository = memberRepository ?? throw new ArgumentNullException(nameof(memberRepository));
            _memberListPresenter = memberListPresenter ?? throw new ArgumentNullException(nameof(memberListPresenter));
        }

        /// <summary>
        ///     制作メンバー一覧を取得して出力します。
        /// </summary>
        public void Execute()
        {
            IReadOnlyList<MemberData> members = _memberRepository.GetAllMembers();
            _memberListPresenter.Present(members ?? Array.Empty<MemberData>());
        }

        private readonly IMemberRepository _memberRepository;
        private readonly IMemberListPresenter _memberListPresenter;
    }
}
