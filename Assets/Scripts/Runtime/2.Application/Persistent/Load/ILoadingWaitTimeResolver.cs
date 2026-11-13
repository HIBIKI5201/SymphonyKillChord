namespace KillChord.Runtime.Application.Persistent.Load
{
    /// <summary>
    ///     遷移元と遷移先のシーンから、ロード画面の最低表示時間を決めるインターフェイス。
    /// </summary>
    public interface ILoadingWaitTimeResolver
    {
        /// <summary>
        ///     遷移元と遷移先に合うロード画面の最低表示時間を返す。
        /// </summary>
        /// <param name="fromSceneName"> 遷移元のシーン名。不明な場合はnull。 </param>
        /// <param name="toSceneName"> 遷移先のシーン名。不明な場合はnull。 </param>
        /// <returns> 最低表示時間（秒）。 </returns>
        float ResolveMinimumDisplayTime(string fromSceneName, string toSceneName);
    }
}
