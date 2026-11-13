using KillChord.Runtime.Domain.OutGame.Screen;
using System.Collections.Generic;

namespace KillChord.Runtime.Application.OutGame.Screen
{
    /// <summary>
    ///     サードパーティ素材の表記リポジトリの抽象。
    /// </summary>
    public interface IThirdPartyNoticeRepository
    {
        /// <summary>
        ///     全てのサードパーティ素材の表記を取得します。
        /// </summary>
        /// <returns> サードパーティ素材の表記の一覧です。 </returns>
        IReadOnlyList<ThirdPartyNoticeData> GetAllNotices();
    }
}
