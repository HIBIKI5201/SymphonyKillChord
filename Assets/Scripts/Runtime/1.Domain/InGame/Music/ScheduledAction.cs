using System;
using System.Threading;

namespace KillChord.Runtime.Domain.InGame.Music
{
    /// <summary>
    ///     スケジュールされたアクションとそのキャンセル情報を保持する構造体。
    /// </summary>
    public readonly struct ScheduledAction
    {
        /// <summary>
        ///     新しいスケジュールアクションを生成する。
        /// </summary>
        /// <param name="action"> 実行するアクション。 </param>
        /// <param name="ct"> キャンセルトークン。 </param>
        public ScheduledAction(Action action, CancellationToken ct)
        {
            Action = action ?? throw new ArgumentNullException(nameof(action));
            CancellationToken = ct;
        }

        /// <summary> 実行するアクション。 </summary>
        public Action Action { get; }
        /// <summary> キャンセルトークン。 </summary>
        public CancellationToken CancellationToken { get; }
    }
}