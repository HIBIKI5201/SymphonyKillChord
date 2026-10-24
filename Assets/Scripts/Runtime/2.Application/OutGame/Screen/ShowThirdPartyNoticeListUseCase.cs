using KillChord.Runtime.Domain.OutGame.Screen;
using System;
using System.Collections.Generic;

namespace KillChord.Runtime.Application.OutGame.Screen
{
    /// <summary>
    ///     サードパーティ素材の表記一覧を取得して出力するユースケース。
    /// </summary>
    public sealed class ShowThirdPartyNoticeListUseCase
    {
        /// <summary>
        ///     ユースケースを初期化します。
        /// </summary>
        /// <param name="noticeRepository"> サードパーティ素材の表記リポジトリです。 </param>
        /// <param name="noticeListPresenter"> サードパーティ素材の表記一覧の出力先です。 </param>
        /// <exception cref="ArgumentNullException"> 引数が null の場合に発生します。 </exception>
        public ShowThirdPartyNoticeListUseCase(
            IThirdPartyNoticeRepository noticeRepository,
            IThirdPartyNoticeListPresenter noticeListPresenter)
        {
            _noticeRepository = noticeRepository ?? throw new ArgumentNullException(nameof(noticeRepository));
            _noticeListPresenter = noticeListPresenter ?? throw new ArgumentNullException(nameof(noticeListPresenter));
        }

        /// <summary>
        ///     サードパーティ素材の表記一覧を取得して出力します。
        /// </summary>
        public void Execute()
        {
            IReadOnlyList<ThirdPartyNoticeData> notices = _noticeRepository.GetAllNotices();
            _noticeListPresenter.Present(notices ?? Array.Empty<ThirdPartyNoticeData>());
        }

        private readonly IThirdPartyNoticeRepository _noticeRepository;
        private readonly IThirdPartyNoticeListPresenter _noticeListPresenter;
    }
}
