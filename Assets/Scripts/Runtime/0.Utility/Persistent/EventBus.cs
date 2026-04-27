using System;
using UnityEngine;

namespace KillChord.Runtime.Utility
{
    /// <summary>
    ///     EventBus
    /// </summary>
    /// <typeparam name="T">イベント定義構造体</typeparam>
    public static class EventBus<T> where T : struct, IEvent
    {
        /// <summary>
        ///     イベントを登録する。
        /// </summary>
        /// <param name="listener"></param>
        public static void Register(Action<T> listener)
        {
            _onEvent += listener;
        }

        /// <summary>
        ///     イベントの登録を解除する。
        /// </summary>
        /// <param name="listener"></param>
        public static void Unregister(Action<T> listener)
        {
            _onEvent -= listener;
        }

        /// <summary>
        ///     イベントを発火する。
        /// </summary>
        /// <param name="eventData"></param>
        public static void Raise(T eventData)
        {
            Action<T> handlers = _onEvent;
            if (handlers is null) return;

            foreach (Action<T> handler in handlers.GetInvocationList())
            {
                try
                {
                    handler(eventData);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        private static event Action<T> _onEvent;
    }
}
