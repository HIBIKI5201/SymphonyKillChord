using KillChord.Runtime.Application.Persistent.SceneManagement;
using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor.Persistent.SceneManagement
{
    /// <summary>
    ///     Viewからのシーン遷移要求を受け取り、シーン遷移サービスを呼び出すコントローラー。
    /// </summary>
    public class SceneTransitionController
    {
        public SceneTransitionController(ISceneTransitionService service)
        {
            _service = service;
        }

        /// <summary>
        ///     シーン遷移を実行する。
        /// </summary>
        /// <param name="fromSceneName"> 遷移元シーン名。 </param>
        /// <param name="toSceneName"> 遷移先シーン名。 </param>
        /// <param name="cancellationToken"> キャンセルトークン。 </param>
        /// <returns> 成功したらtrue。 </returns>
        public async Task<bool> ChangeSceneAsync(
            string fromSceneName,
            string toSceneName,
            CancellationToken cancellationToken)
        {
            return await _service.ChangeSceneAsync(fromSceneName, toSceneName, cancellationToken);
        }

        /// <summary>
        ///     遷移元を残したまま遷移先シーンをAdditiveロードし、ActiveSceneを切り替える。
        /// </summary>
        public async Task<bool> LoadAdditiveAndSetActiveAsync(
            string toSceneName,
            CancellationToken cancellationToken)
        {
            return await _service.LoadAdditiveAndSetActiveAsync(
                toSceneName,
                cancellationToken);
        }

        /// <summary>
        ///     対象シーンをUnloadし、ActiveSceneを指定シーンへ戻す。
        /// </summary>
        public async Task<bool> UnloadAndSetActiveAsync(
            string unloadSceneName,
            string activeSceneName,
            CancellationToken cancellationToken)
        {
            return await _service.UnloadAndSetActiveAsync(
                unloadSceneName,
                activeSceneName,
                cancellationToken);
        }

        private readonly ISceneTransitionService _service;
    }
}
