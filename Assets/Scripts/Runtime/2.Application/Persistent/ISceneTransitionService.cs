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
        /// <param name="cancellationToken"> キャンセルトークン。 </param>
        /// <returns> 成功したらtrue。 </returns>
        ValueTask<bool> ChangeSceneAsync(
            string fromSceneName,
            string toSceneName,
            CancellationToken cancellationToken);
    }
}
