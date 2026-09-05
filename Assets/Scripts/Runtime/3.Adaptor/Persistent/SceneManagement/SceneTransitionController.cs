using KillChord.Runtime.Application.Persistent.SceneManagement;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor.Persistent.SceneManagement
{
    /// <summary>
    ///     Viewからのシーン遷移要求を受け取り、シーン遷移サービスを呼び出すコントローラー。
    /// </summary>
    public class SceneTransitionController
    {
        public SceneTransitionController(
            SceneTransitionUsecase usecase,
            CancellationToken persistentLifetimeToken = default)
        {
            _useCase = usecase
                ?? throw new ArgumentNullException(nameof(usecase));
            _persistentLifetimeToken = persistentLifetimeToken;
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
            return await _useCase.ChangeSceneAsync(fromSceneName, toSceneName, cancellationToken);
        }

        public Task<bool> ChangeSceneKeepingLoadingAsync(
            string fromSceneName,
            string toSceneName,
            CancellationToken cancellationToken)
        {
            return _useCase.ChangeSceneKeepLoadingAsync(
                fromSceneName,
                toSceneName,
                cancellationToken);
        }

        /// <summary>
        ///     シーンをAdditiveロードする。
        /// </summary>
        public Task<bool> LoadAdditiveAsync(
            string sceneName,
            CancellationToken cancellationToken)
        {
            return _useCase.LoadAdditiveAsync(
                sceneName,
                cancellationToken);
        }

        /// <summary>
        ///     シーンをアンロードする。
        /// </summary>
        public async Task<bool> UnloadAsync(
            string sceneName,
            CancellationToken cancellationToken)
        {
            return await _useCase.UnloadAsync(
                sceneName,
                cancellationToken);
        }

        /// <summary>
        ///     常駐シーンの寿命に従ってシーンをアンロードする。
        ///     呼び出し元のシーンがアンロードされても、アンロード完了後の後処理まで実行するために使用する。
        /// </summary>
        /// <param name="sceneName"> アンロードするシーン名。 </param>
        /// <returns> 成功した場合はtrue。 </returns>
        public Task<bool> UnloadWithPersistentLifetimeAsync(string sceneName)
        {
            return _useCase.UnloadAsync(sceneName, _persistentLifetimeToken);
        }

        /// <summary>
        ///     対象シーンをUnloadし、ActiveSceneを指定シーンへ戻す。
        /// </summary>
        public Task<bool> UnloadAndSetActiveAsync(
            string unloadSceneName,
            string activeSceneName,
            CancellationToken cancellationToken)
        {
            return _useCase.UnloadAndSetActiveAsync(
                unloadSceneName,
                activeSceneName,
                cancellationToken);
        }

        private readonly SceneTransitionUsecase _useCase;
        private readonly CancellationToken _persistentLifetimeToken;
    }
}
