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
        ///     遷移先の初期化がロード画面を引き継いで閉じるかを設定します。
        ///     <para> 途中復帰でバトルパートへ進む場合のように、遷移先がロード画面を閉じる場合にtrueにします。 </para>
        /// </summary>
        /// <param name="isKeepLoading"> 遷移先がロード画面を閉じる場合はtrueです。 </param>
        public void SetKeepLoadingOnStart(bool isKeepLoading)
        {
            _isKeepLoadingOnStart = isKeepLoading;
        }

        /// <summary>
        ///     ゲームを開始する。
        ///     設定された遷移先シーン（アウトゲーム・シナリオ・途中復帰先など）に遷移する。
        /// </summary>
        /// <param name="currentSceneName"> 現在のシーン名。 </param>
        /// <param name="targetSceneName"> 遷移先のシーン名。 </param>
        /// <param name="token"> Title View の寿命に従うキャンセルトークン。 </param>
        /// <returns> 遷移が成功したかどうか。 </returns>
        public async Task<bool> StartGameAsync(string currentSceneName, string targetSceneName, CancellationToken token)
        {
            // 多重呼び出しを防ぐため、すでに遷移中の場合は false を返す。
            if (_isActivate || _sceneTransitionController.HasScenarioBattleSortie)
            {
                return false;
            }

            _isActivate = true;
            bool isSuccess = false;

            try
            {
                // 遷移先の読み込みと Title のアンロードを1回のロード画面で行い、途中で画面を開けない。
                // Title のアンロードは自身の View を破棄するため、View のトークンでは待機しない。
                // 常駐シーンの寿命で完了を待機し、ロードセッションとシーン初期化追跡を正常終了させる。
                // 遷移先がロード画面を閉じる場合は、開いたまま引き渡す。
                isSuccess = _isKeepLoadingOnStart
                    ? await _sceneTransitionController
                        .ChangeSceneKeepingLoadingWithPersistentLifetimeAsync(
                            currentSceneName,
                            targetSceneName)
                    : await _sceneTransitionController
                        .LoadAdditiveThenUnloadWithPersistentLifetimeAsync(
                            targetSceneName,
                            currentSceneName,
                            token);

                if (!isSuccess)
                {
                    _isActivate = false;
                    return false;
                }

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

        /// <summary> 遷移先がロード画面を引き継いで閉じる場合はtrueです。 </summary>
        private bool _isKeepLoadingOnStart;
    }
}
