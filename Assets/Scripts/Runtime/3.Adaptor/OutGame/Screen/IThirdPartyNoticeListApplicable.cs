using System.Collections.Generic;

namespace KillChord.Runtime.Adaptor.OutGame.Screen
{
    /// <summary>
    ///     サードパーティ素材の表記一覧を View へ反映できることを表す抽象。
    /// </summary>
    public interface IThirdPartyNoticeListApplicable
    {
        /// <summary>
        ///     サードパーティ素材の表記一覧を View へ反映します。
        /// </summary>
        /// <param name="notices"> 反映するサードパーティ素材の表記 DTO の一覧です。 </param>
        void ApplyThirdPartyNoticeList(IReadOnlyList<ThirdPartyNoticeViewDTO> notices);
    }
}
