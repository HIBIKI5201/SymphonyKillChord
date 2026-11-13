using KillChord.Runtime.Domain.OutGame.Screen;
using System.Collections.Generic;

namespace KillChord.Runtime.Application.OutGame.Screen
{
    /// <summary>
    ///     サードパーティ素材の表記一覧の出力先の抽象。
    /// </summary>
    public interface IThirdPartyNoticeListPresenter
    {
        /// <summary>
        ///     サードパーティ素材の表記一覧を出力します。
        /// </summary>
        /// <param name="notices"> 出力するサードパーティ素材の表記の一覧です。 </param>
        void Present(IReadOnlyList<ThirdPartyNoticeData> notices);
    }
}
