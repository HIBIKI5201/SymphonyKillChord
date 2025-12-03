using System;
using System.Threading;
using UnityEngine;

namespace Mock.MusicBattle.MusicSync
{
    /// <summary>
    ///     予約内容
    /// </summary>
    public class ScheduledAction
    {
        /// <summary>発火タイミング</summary>
        public double ExecuteBeat { get; private set; }
        /// <summary>実行アクション</summary>
        public Action Action { get; private set; }
        public ScheduledAction(double executeBeat, Action action)
        {
            ExecuteBeat = executeBeat;
            Action = action;
        }
    }
}
