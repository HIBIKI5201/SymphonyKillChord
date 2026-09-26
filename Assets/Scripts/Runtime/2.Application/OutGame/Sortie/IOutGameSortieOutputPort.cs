using KillChord.Runtime.Domain.OutGame.StageSelect;
using System.Threading.Tasks;

namespace KillChord.Runtime.Application.OutGame.Sortie
{
    /// <summary>
    ///     出撃ボタンを押したときの出撃処理の出力ポート。
    /// </summary>
    public interface IOutGameSortieOutputPort
    {
        /// <summary>
        ///     戦闘準備画面を介さずにバトル開始を要求します。
        /// </summary>
        void StartBattle();

        /// <summary>
        ///     受付確定後に予約を消費し、シナリオと旧OutGameを終了して出撃します。
        /// </summary>
        Task<ScenarioBattleSortieResult> StartBattleFromScenarioAsync(
            string scenarioSceneName,
            string outGameSceneName,
            BattleStageDefinition battleStageDefinition,
            int scenarioSelectionRevision,
            bool isInputValid);

        /// <summary>
        ///     ホーム画面の表示を要求します。
        /// </summary>
        void ShowHomeScreen();

        /// <summary>
        ///    シナリオステージの出撃処理を要求する。
        /// </summary>
        /// <param name="isActive"> シナリオステージの出撃処理を有効にするかどうか。 </param>
        void SetOutGameActiveForScenario(bool isActive);
    }
}
