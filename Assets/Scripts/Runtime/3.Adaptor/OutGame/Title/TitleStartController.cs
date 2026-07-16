using KillChord.Runtime.Adaptor.Persistent.SceneManagement;
using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor.OutGame.Title
{
    /// <summary>
    ///     タイトルシーンでゲームの開始を制御するコントローラー。
    /// </summary>
    public class TitleStartController
    {
        /// <summary>
        ///     コントローラーを初期化します。
        /// </summary>
        /// <param name="sceneTransitionController"> シーン遷移コントローラーです。 </param>
        public TitleStartController(SceneTransitionController sceneTransitionController)
        {
            _sceneTransitionController = sceneTransitionController;
        }

        /// <summary>
        ///     タイトルから直接遷移するチュートリアル戦闘シーンを設定します。
        /// </summary>
        /// <param name="tutorialBattleSceneName"> 遷移先シーン名です。 </param>
        public void SetTutorialBattleTarget(string tutorialBattleSceneName)
        {
            _tutorialBattleSceneName = tutorialBattleSceneName;
        }

        /// <summary>
        ///     タイトルから直接遷移するチュートリアル戦闘シーン設定を解除します。
        /// </summary>
        public void ClearTutorialBattleTarget()
        {
            _tutorialBattleSceneName = string.Empty;
        }

        /// <summary>
        ///     ゲームを開始する。
        ///     アウトゲームシーンに遷移する。
        /// </summary>
        /// <param name="currentSceneName"> 現在のシーン名。 </param>
        /// <param name="targetSceneName"> 遷移先のシーン名。 </param>
        /// <param name="token"> キャンセルトークン。 </param>
        /// <returns> 遷移が成功したかどうか。 </returns>
        public async Task<bool> StartGameAsync(string currentSceneName, string targetSceneName, CancellationToken token)
        {
            // 多重呼び出しを防ぐため、すでに遷移中の場合は false を返す。
            if (_isActivate)
            {
                return false;
            }

            _isActivate = true;
            bool isSuccess = false;

            try
            {
                if (!string.IsNullOrWhiteSpace(_tutorialBattleSceneName))
                {
                    isSuccess = await _sceneTransitionController.ChangeSceneKeepingLoadingAsync(
                        currentSceneName,
                        _tutorialBattleSceneName,
                        token);

                    if (!isSuccess)
                    {
                        _isActivate = false;
                    }

                    return isSuccess;
                }

                isSuccess =
                    await _sceneTransitionController.LoadAdditiveAsync(targetSceneName, token);

                if (!isSuccess)
                {
                    _isActivate = false;
                    return false;
                }

                await _sceneTransitionController.UnloadAsync(currentSceneName, token);

                return true;
            }
            catch
            {
                _isActivate = false;
                throw;
            }
        }

        private readonly SceneTransitionController _sceneTransitionController;

        /// <summary>
        ///    遷移中かどうかを示すフラグ。
        ///    多重呼び出しを防ぐために使用する。
        /// </summary>
        private bool _isActivate = false;
        private string _tutorialBattleSceneName = string.Empty;
    }
}
