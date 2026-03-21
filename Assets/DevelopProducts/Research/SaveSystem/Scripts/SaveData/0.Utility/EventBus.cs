using System;
using System.Linq;
using UnityEngine;
namespace Research.SaveSystem
{

    /// <summary>
    ///     EventBus<br/>
    ///     イベントを構造体形式にすることで、イベントがstackに割り当てられ、アクセス速度が速く、GCも避けられる。<br/>
    ///     C#の静的ジェネリクス特性によって、デリゲートを取得する時、通常のDictionaryより早い。
    /// </summary>
    /// <typeparam name="T"></typeparam>
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