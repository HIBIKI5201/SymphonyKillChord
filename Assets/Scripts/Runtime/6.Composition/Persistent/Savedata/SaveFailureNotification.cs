using KillChord.Runtime.View.Persistent.Load;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using System.Threading;
using UnityEngine;

namespace KillChord.Runtime.Composition.Persistent.Savedata
{
    /// <summary>
    ///     セーブデータの保存に失敗したことを、常駐の中央通知でプレイヤーへ伝える。
    ///     どのデータの保存に失敗したときも、同じ文言で通知する。
    /// </summary>
    public static class SaveFailureNotification
    {
        /// <summary>
        ///     保存に失敗したことを通知する。通知を表示できないときは何もしない。
        ///     例外はこのメソッドの中でログに出すため、呼び出し側は待たずに捨ててよい。
        /// </summary>
        /// <param name="context"> ログに添えるオブジェクト。 </param>
        /// <param name="cancellationToken"> 表示を中断するトークン。 </param>
        public static async Awaitable ShowAsync(UnityEngine.Object context, CancellationToken cancellationToken)
        {
            try
            {
                // 通知を表示できない場合は、保存処理側のログだけに任せる。
                if (!ServiceLocator.TryGetInstance(out EventNotificationView notificationView)
                    || notificationView == null
                    || notificationView.IsVisible
                    || !notificationView.isActiveAndEnabled)
                {
                    return;
                }

                await notificationView.ShowAsync(SAVE_FAILED_NOTIFICATION_ENTRY, cancellationToken);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, context);
            }
        }

        /// <summary> 保存失敗を伝える通知のUICommonエントリ。 </summary>
        private const string SAVE_FAILED_NOTIFICATION_ENTRY = "ui.notification.save_failed";
    }
}
