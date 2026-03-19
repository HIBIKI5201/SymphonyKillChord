using UnityEngine;
namespace Research.SaveSystem
{
    /// <summary>
    ///     asyncメソッドをForgetできるようにする拡張
    /// </summary>
    public static class AwaitableExtension
    {
        public static async void Forget(this Awaitable awaitable)
        {
            await awaitable;
        }
    }
}