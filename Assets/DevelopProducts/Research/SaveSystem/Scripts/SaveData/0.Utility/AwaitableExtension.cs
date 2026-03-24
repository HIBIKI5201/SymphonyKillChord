using UnityEngine;
namespace Research.SaveSystem
{
    /// <summary>
    ///     asyncメソッドをForgetできるようにする拡張するクラス。
    /// </summary>
    public static class AwaitableExtension
    {
        public static async void Forget(this Awaitable awaitable)
        {
            await awaitable;
        }
    }
}