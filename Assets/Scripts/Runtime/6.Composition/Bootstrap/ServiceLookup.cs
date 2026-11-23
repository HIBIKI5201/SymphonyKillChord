using SymphonyFramework.Services;

namespace KillChord.Runtime.Composition.Bootstrap
{
    /// <summary>
    ///     Partituraから、登録が無ければnullを返す形でサービスを取り出す。
    ///     SymphonyFramework 6.x の ServiceLocator.GetInstance と同じ挙動を保つために置く。
    /// </summary>
    internal static class ServiceLookup
    {
        /// <summary>
        ///     登録済みのサービスを返す。登録が無い場合はnullを返す。
        /// </summary>
        /// <typeparam name="T"> 取り出すサービスの型。 </typeparam>
        /// <returns> 登録済みのサービス。登録が無い場合はnull。 </returns>
        public static T GetOrNull<T>() where T : class
        {
            // 未登録を例外にせず、呼び出し側のnull判定へ任せる。
            return Partitura.TryGet(out T result) ? result : null;
        }
    }
}
