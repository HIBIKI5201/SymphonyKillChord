using KillChord.Runtime.Application.Persistent.SceneManagement;
using KillChord.Runtime.Domain.OutGame.StageSelect;
using System.Threading;
using System.Threading.Tasks;

namespace KillChord.Runtime.Application.OutGame.Sortie
{
    /// <summary>
    ///     アウトゲームから各ステージへ出撃する流れを制御するユースケース。
    /// </summary>
    public sealed class OutGameSortieUseCase
    {
        /// <summary>
        ///     ユースケースを初期化する。
        /// </summary>
        /// <param name="sceneTransitionService"> シーン遷移サービス。 </param>
        /// <param name="outputPort"> 出力ポート。 </param>
        public OutGameSortieUseCase(
               ISceneTransitionService sceneTransitionService,
               IOutGameSortieOutputPort outputPort)
        {
            _sceneTransitionService = sceneTransitionService;
            _outputPort = outputPort;
        }

        /// <summary>
        ///    ステージタイプに応じた処理を実行する。
        ///    バトルステージの場合は戦闘準備画面を表示する。
        ///    シナリオステージの場合はシーン遷移を行う。
        /// </summary>
        /// <param name="stageType"> ステージの種類。 </param>
        /// <param name="fromSceneName"> 現在のシーン名。 </param>
        /// <param name="targetSceneName"> 遷移先のシーン名。 </param>
        /// <param name="cancellationToken"> キャンセルトークン。 </param>
        /// <returns> 出撃が成功したかどうか。 </returns>
        public async Task<bool> RequestSortieAsync(
            StageType stageType,
            string fromSceneName,
            string targetSceneName,
            CancellationToken cancellationToken)
        {
            if (stageType == StageType.Battle)
            {
                _outputPort.ShowBattlePreparationScreen(targetSceneName);
                return true;
            }

            _outputPort.SetOutGameActiveForScenario(false);

            bool success = await _sceneTransitionService.LoadAdditiveAndSetActiveAsync(
                targetSceneName,
                cancellationToken);

            if (!success)
            {
                _outputPort.SetOutGameActiveForScenario(true);
            }

            return success;
        }

        private readonly ISceneTransitionService _sceneTransitionService;
        private readonly IOutGameSortieOutputPort _outputPort;
    }
}
