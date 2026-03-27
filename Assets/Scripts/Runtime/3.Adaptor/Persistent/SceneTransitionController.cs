using KillChord.Runtime.Application;
using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor
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
        public Task<bool> ChangeSceneAsync(
            string fromSceneName,
            string toSceneName,
            CancellationToken cancellationToken)
        {
            return _service.ChangeSceneAsync(fromSceneName, toSceneName, cancellationToken);
        }

        private readonly ISceneTransitionService _service;
    }
}
