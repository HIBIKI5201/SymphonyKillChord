using System;
using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Application.Persistent.SceneManagement
{
    /// <summary>
    ///     シーン遷移処理を表すインターフェース。
    /// </summary>
    public interface ISceneTransitionService
    {
        /// <summary>
        ///     指定したシーンへ遷移する。
        /// </summary>
        /// <param name="fromSceneName"> 遷移元シーン名。 </param>
        /// <param name="toSceneName"> 遷移先シーン名。 </param>
        /// <param name="progress"> シーン遷移全体の進捗通知先。 </param> 
        /// <param name="cancellationToken"> キャンセルトークン。 </param>
        /// <returns> 成功したらtrue。 </returns>
        Task<bool> ChangeSceneAsync(
            string fromSceneName,
            string toSceneName,
            IProgress<float> progress,
            CancellationToken cancellationToken);

        /// <summary>
        ///     シーンをAdditiveロードする。
        /// </summary>
        Task<bool> LoadAdditiveAsync(
            string sceneName,
            IProgress<float> progress,
            CancellationToken cancellationToken);

        /// <summary>
        ///     シーンをアンロードする。
        /// </summary>
        Task<bool> UnloadAsync(
            string sceneName,
            IProgress<float> progress,
            CancellationToken cancellationToken);

        /// <summary>
        ///    指定したシーンを加算ロードし、アクティブにする。
        /// </summary>
        /// <param name="toSceneName"> 加算ロードするシーン名。 </param>
        /// <param name="progress"> シーン遷移全体の進捗通知先。 </param>
        /// <param name="cancellationToken"> キャンセルトークン。 </param>
        /// <returns> 成功したらtrue。 </returns>
        Task<bool> LoadAdditiveAndSetActiveAsync(
            string toSceneName,
            IProgress<float> progress,
            CancellationToken cancellationToken);

        /// <summary>
        ///    指定したシーンをアンロードし、別のシーンをアクティブにする。
        /// </summary>
        /// <param name="unloadSceneName"> アンロードするシーン名。 </param>
        /// <param name="activeSceneName"> アクティブにするシーン名。 </param>
        /// <param name="cancellationToken"> キャンセルトークン。 </param>
        /// <returns> 成功したらtrue。 </returns>
        Task<bool> UnloadAndSetActiveAsync(
            string unloadSceneName,
            string activeSceneName,
            IProgress<float> progress,
            CancellationToken cancellationToken);

        /// <summary>
        ///     指定したシーンをアンロードして再読み込みする。
        /// </summary>
        /// <param name="sceneName"> 再読み込みするシーン名。 </param>
        /// <param name="progress"> 進捗通知先。 </param>
        /// <param name="cancellationToken"> キャンセルトークン。 </param>
        /// <returns> 成功した場合はtrue。 </returns>
        Task<bool> ReloadSceneAsync(
            string sceneName,
            IProgress<float> progress,
            CancellationToken cancellationToken);
    }
}
