using KillChord.Runtime.Adaptor.InGame.Result;
using KillChord.Runtime.Adaptor.InGame.StageSelect;
using KillChord.Runtime.Domain.Persistent.Savedata;
using SymphonyFrameWork.System.SaveSystem;

namespace KillChord.Demo
{
    /// <summary>
    ///     制限時間切れ、または最終ミッションクリア後のリザルトを体験版終了へ接続します。
    /// </summary>
    public sealed class DemoStageResultExitPolicy : IStageResultExitPolicy
    {
        /// <summary>
        ///     ポリシーを生成します。
        /// </summary>
        /// <param name="sessionState"> 継続中の体験版セッション状態です。 </param>
        /// <param name="config"> 体験版設定です。 </param>
        public DemoStageResultExitPolicy(DemoSessionState sessionState, DemoExperienceConfig config)
        {
            _sessionState = sessionState;
            _config = config;
        }

        /// <inheritdoc />
        public bool TryGetDestinationScene(
            StageResultExitAction action,
            SelectedBattleStageState selectedBattleStageState,
            out string destinationSceneName)
        {
            destinationSceneName = string.Empty;
            if (selectedBattleStageState == null
                || !selectedBattleStageState.HasSelectedBattleStage
                || string.IsNullOrWhiteSpace(_config.EndSceneName))
            {
                return false;
            }

            int currentStageId = selectedBattleStageState.CurrentStageDefinition.StageId.Value;
            bool isFinalStageCleared = currentStageId == _config.FinalStageId
                && SaveStore.IsLoaded<SaveData>()
                && SaveStore.Get<SaveData>().StageProgress.IsStageCleared(currentStageId);
            if (!_sessionState.IsOverallTimeExpired && !isFinalStageCleared)
            {
                return false;
            }

            destinationSceneName = _config.EndSceneName;
            return true;
        }

        private readonly DemoSessionState _sessionState;
        private readonly DemoExperienceConfig _config;
    }
}
