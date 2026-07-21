using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.Utility.Persistent
{
    /// <summary>
    ///     EventBus&lt;T&gt;はジェネリック型のため[RuntimeInitializeOnLoadMethod]を直接付けられない。
    ///     各EventBus&lt;T&gt;の静的コンストラクタがここへリセット処理を登録し、
    ///     非ジェネリックな本クラスがまとめて呼び出す。
    /// </summary>
    internal static class EventBusResetRegistry
    {
        /// <summary>
        ///     EventBus&lt;T&gt;のリセット処理を登録する。
        /// </summary>
        /// <param name="resetAction"> 登録するリセット処理です。 </param>
        internal static void Register(Action resetAction)
        {
            _resetActions.Add(resetAction);
        }

        /// <summary>
        ///     登録済みの全EventBus&lt;T&gt;をリセットする。
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetAll()
        {
            foreach (Action resetAction in _resetActions)
            {
                resetAction();
            }
        }

        private static readonly List<Action> _resetActions = new();
    }
}
