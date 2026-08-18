using KillChord.Runtime.Domain.OutGame.Screen;
using System.Collections.Generic;

namespace KillChord.Runtime.Application.OutGame.Screen
{
    /// <summary>
    ///     制作メンバー一覧の出力先の抽象。
    /// </summary>
    public interface IMemberListPresenter
    {
        /// <summary>
        ///     制作メンバー一覧を出力します。
        /// </summary>
        /// <param name="members"> 出力する制作メンバー情報の一覧です。 </param>
        void Present(IReadOnlyList<MemberData> members);
    }
}
