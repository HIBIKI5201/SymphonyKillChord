using System;
using System.Threading;
using UnityEngine;

namespace Mock.MusicBattle.MusicSync
{
    /// <summary>
    ///     音楽同期システムで予約されたアクションの情報を保持するクラス。
    /// </summary>
    public class ScheduledAction
    {
        /// <summary> アクションが発火する拍のタイミング。 </summary>
        public double ExecuteBeat { get; private set; }
        /// <summary> 実行されるアクション。 </summary>
        public Action Action { get; private set; }

        /// <summary>
        ///     <see cref="ScheduledAction"/>クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="executeBeat">アクションが発火する拍のタイミング。</param>
        /// <param name="action">実行されるアクション。</param>
        public ScheduledAction(double executeBeat, Action action)
        {
            ExecuteBeat = executeBeat;
            Action = action;
        }
    }
}

