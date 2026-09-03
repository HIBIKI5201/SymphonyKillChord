using KillChord.Runtime.Adaptor.InGame.StageSelect;
using KillChord.Runtime.Application.Persistent.SceneManagement;
using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Adaptor.InGame.state
{
    public sealed class StageRestartController
    {
        /// <summary>
        ///     コンストラクタ。
        /// </summary>
        public StageRestartController(
            SceneTransitionUsecase sceneTransitionUsecase,
            SelectedBattleStageState selectedBattleStageState)
        {
            _sceneTransitionUsecase = sceneTransitionUsecase;
            _selectedBattleStageState = selectedBattleStageState;
        }

        /// <summary>
        ///     現在のシーン(戦闘)の再読み込みを行う。
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<bool> RestartAsync(CancellationToken cancellationToken)
        {
            if (!_selectedBattleStageState.HasSelectedBattleStage)
            {
                // 戦闘ステージが未選択の場合は、再起動処理を行わずに false を返す
                return Task.FromResult(false);
            }

            // 戦闘ステージが選択されている場合は、シーンのアンロードとリロードを行う
            return _sceneTransitionUsecase.UnloadThenReloadSceneAsync(
                _selectedBattleStageState.BattleSceneName,
                _selectedBattleStageState.BattleSceneName,
                cancellationToken);
        }

        private readonly SceneTransitionUsecase _sceneTransitionUsecase;
        private readonly SelectedBattleStageState _selectedBattleStageState;
    }
}
