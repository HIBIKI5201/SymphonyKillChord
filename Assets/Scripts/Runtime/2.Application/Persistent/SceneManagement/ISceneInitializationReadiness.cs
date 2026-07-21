using System.Threading;
using UnityEngine;

namespace KillChord.Runtime.Application.Persistent.SceneManagement
{
    /// <summary>
    ///     シーン初期化の追跡と完了待機を行う契約です。
    /// </summary>
    public interface ISceneInitializationReadiness
    {
        /// <summary>
        ///     対象シーンの初期化追跡を開始します。
        /// </summary>
        /// <param name="sceneName"> 追跡するシーン名です。 </param>
        void BeginTracking(string sceneName);

        /// <summary>
        ///     対象シーンの初期化結果を通知します。
        /// </summary>
        /// <param name="sceneName"> 完了したシーン名です。 </param>
        /// <param name="isSuccess"> 初期化に成功した場合はtrueです。 </param>
        void Complete(string sceneName, bool isSuccess);

        /// <summary>
        ///     対象シーンの追跡状態を解除します。
        /// </summary>
        /// <param name="sceneName"> 追跡を解除するシーン名です。 </param>
        void Clear(string sceneName);

        /// <summary>
        ///     対象シーンの初期化完了を待機します。
        /// </summary>
        /// <param name="sceneName"> 待機するシーン名です。 </param>
        /// <param name="cancellationToken"> キャンセルトークンです。 </param>
        /// <returns> 初期化に成功した場合はtrueです。 </returns>
        Awaitable<bool> WaitForReadyAsync(
            string sceneName,
            CancellationToken cancellationToken);
    }
}
