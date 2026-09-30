using KillChord.Runtime.Adaptor.OutGame.Scenario;
using KillChord.Runtime.Adaptor.OutGame.StageSelect;
using KillChord.Runtime.Adaptor.Persistent.SceneManagement;
using KillChord.Runtime.Application.OutGame.Sortie;
using KillChord.Runtime.Application.Persistent.Load;
using KillChord.Runtime.Composition.Persistent.Input;
using KillChord.Runtime.Composition.Persistent.SceneManagement;
using KillChord.Runtime.Domain.OutGame.StageSelect;
using KillChord.Runtime.View.OutGame.Screen;
using KillChord.Runtime.View.Persistent.Input;
using System.Threading.Tasks;

namespace KillChord.Runtime.Composition.OutGame.Sortie
{
    /// <summary>
    ///     出撃ユースケースの出力ポートの実装クラス。
    ///     Application と View に依存しているため Composition に配置している。
    /// </summary>
    public sealed class OutGameSortieOutputPort : IOutGameSortieOutputPort
    {
        /// <summary>
        ///    OutGameSortieOutputPort を初期化します。
        /// </summary>
        /// <param name="outGameUIEvent"> OutGameUIEvent のインスタンス。 </param>
        /// <param name="inputComposition"> InputComposition のインスタンス。 </param>
        public OutGameSortieOutputPort(
            OutGameUIEvent outGameUIEvent,
            InputComposition inputComposition,
            SceneTransitionController transition,
            ILoadingOperationExecutor executor,
            SceneTransitionInitializer transitionInitializer)
        {
            _outGameUIEvent = outGameUIEvent;
            _inputComposition = inputComposition;
            _sortieExecutor = new ScenarioBattleSortieExecutor(transition, executor, transitionInitializer);
        }

        /// <summary>
        ///     バトル開始イベントを通知します。
        /// </summary>
        public void StartBattle()
        {
            _outGameUIEvent.OnStartGame?.Invoke();
        }

        /// <summary>
        ///     ホーム画面の表示を要求します。
        /// </summary>
        public void ShowHomeScreen()
        {
            _outGameUIEvent.OnShownHomeScreen?.Invoke();
        }

        /// <summary>
        ///     シナリオ再生状態に合わせてOutGameの表示と入力を切り替えます。
        /// </summary>
        /// <param name="isActive"> OutGameを有効にする場合はtrueです。 </param>
        public void SetOutGameActiveForScenario(bool isActive)
        {
            _outGameUIEvent.OnOutGameUiVisibilityChanged?.Invoke(isActive);

            if (_inputComposition == null)
            {
                return;
            }

            if (isActive)
            {
                _inputComposition.GetInputMapController.EnableCommonWith(InputMapNames.OutGame);
                return;
            }

            _inputComposition.GetInputMapController.EnableCommonWith(InputMapNames.Scenario);
        }

        /// <summary>
        ///     初回Build順を変えず、Readyで公開済みStateへ接続します。
        /// </summary>
        public void ConnectScenarioBattleSortie(PendingNodeTransitionState pendingState,
            SelectedScenarioState selectedState, BattleSortieSelectionService selectionService)
        {
            _pendingState = pendingState;
            _selectedState = selectedState;
            _selectionService = selectionService;
        }

        /// <summary>
        ///     同期受付の内側で予約を消費し、両旧シーンを終了して出撃します。
        /// </summary>
        public Task<ScenarioBattleSortieResult> StartBattleFromScenarioAsync(
            string scenarioSceneName, string outGameSceneName, BattleStageDefinition definition,
            int scenarioSelectionRevision, bool isInputValid)
        {
            // OutGame の上で再生したシナリオなので、終了するシーンと帰還先はどちらも OutGame になる。
            return _sortieExecutor.ExecuteAsync(
                _pendingState, _selectedState, _selectionService, scenarioSceneName, outGameSceneName,
                outGameSceneName, definition, scenarioSelectionRevision, isInputValid);
        }

        private readonly OutGameUIEvent _outGameUIEvent;
        private readonly InputComposition _inputComposition;
        private readonly ScenarioBattleSortieExecutor _sortieExecutor;
        private PendingNodeTransitionState _pendingState;
        private SelectedScenarioState _selectedState;
        private BattleSortieSelectionService _selectionService;
    }
}
