using System;
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
            _onEvent?.Invoke(eventData);
        }

        private static event Action<T> _onEvent;
    }
}