using KillChord.Runtime.Application.OutGame.Screen;
using KillChord.Runtime.Domain.OutGame.Screen;
using System;
using System.Collections.Generic;

namespace KillChord.Runtime.Adaptor.OutGame.Screen
{
    /// <summary>
    ///     サードパーティ素材の表記一覧を DTO へ変換して View へ橋渡しする Presenter。
    /// </summary>
    public sealed class ThirdPartyNoticeListPresenter : IThirdPartyNoticeListPresenter
    {
        /// <summary>
        ///     Presenter を初期化します。
        /// </summary>
        /// <param name="noticeListApplicable"> サードパーティ素材の表記一覧の反映先です。 </param>
        /// <exception cref="ArgumentNullException"> 引数が null の場合に発生します。 </exception>
        public ThirdPartyNoticeListPresenter(IThirdPartyNoticeListApplicable noticeListApplicable)
        {
            _noticeListApplicable = noticeListApplicable ?? throw new ArgumentNullException(nameof(noticeListApplicable));
        }

        /// <summary>
        ///     サードパーティ素材の表記一覧を DTO へ変換して出力します。
        /// </summary>
        /// <param name="notices"> 出力するサードパーティ素材の表記の一覧です。 </param>
        public void Present(IReadOnlyList<ThirdPartyNoticeData> notices)
        {
            if (notices == null)
            {
                _noticeListApplicable.ApplyThirdPartyNoticeList(Array.Empty<ThirdPartyNoticeViewDTO>());
                return;
            }

            var noticeViewDTOs = new ThirdPartyNoticeViewDTO[notices.Count];
            for (int i = 0; i < notices.Count; i++)
            {
                ThirdPartyNoticeData notice = notices[i];
                noticeViewDTOs[i] = new ThirdPartyNoticeViewDTO(
                    notice.Name,
                    notice.Holder,
                    notice.License,
                    notice.Text);
            }

            _noticeListApplicable.ApplyThirdPartyNoticeList(noticeViewDTOs);
        }

        private readonly IThirdPartyNoticeListApplicable _noticeListApplicable;
    }
}
