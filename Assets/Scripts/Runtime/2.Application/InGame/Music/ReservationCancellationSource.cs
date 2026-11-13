using System;
using System.Threading;

namespace KillChord.Runtime.Application.InGame.Music
{
    /// <summary>
    ///     拍の予約に使うキャンセルトークンを管理する。
    ///     攻撃予約・砲弾予約など、予約を張り直すたびに前の予約を取り消す処理で共通して使う。
    /// </summary>
    public sealed class ReservationCancellationSource : IDisposable
    {
        /// <summary>
        ///     前の予約をキャンセルし、新しい予約用のトークンを発行する。
        /// </summary>
        /// <returns> 新しい予約に渡すキャンセルトークン。 </returns>
        public CancellationToken Renew()
        {
            Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            return _cancellationTokenSource.Token;
        }

        /// <summary>
        ///     発行中のトークンをキャンセルして破棄する。発行していない場合は何もしない。
        /// </summary>
        public void Cancel()
        {
            if (_cancellationTokenSource == null)
            {
                return;
            }

            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }

        /// <summary>
        ///     発行中のトークンをキャンセルして破棄する。
        /// </summary>
        public void Dispose()
        {
            Cancel();
        }

        private CancellationTokenSource _cancellationTokenSource;
    }
}
