using KillChord.Runtime.Adaptor.InGame.Mission;
using KillChord.Runtime.Adaptor.InGame.Result;
using KillChord.Runtime.Adaptor.InGame.Sequence;
using KillChord.Runtime.Adaptor.InGame.StageSelect;
using KillChord.Runtime.Application.Persistent.SceneManagement;
using KillChord.Runtime.Composition.InGame.Bootstrap;
using KillChord.Runtime.Composition.InGame.Result;
using KillChord.Runtime.Composition.InGame.Sequence;
using KillChord.Runtime.View.InGame.UI;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.UI
{
    /// <summary>
    ///     ポーズウィンドウを戦闘ポーズとシーン遷移に結合するクラス。
    /// </summary>
    public sealed class PauseWindowInitializer : InGameInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(PauseWindowInitializer);

        /// <summary> 実行順です。 </summary>
        public override int Order => 1010;

        /// <summary>
        ///     ポーズ状態とボタン操作を結合してポーズウィンドウを構築します。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Ready()
        {
            if (_pauseWindowView == null)
            {
                Debug.LogError($"[{nameof(PauseWindowInitializer)}] {nameof(PauseWindowView)} が設定されていません。", this);
                return false;
            }

            if (!ServiceLocator.TryGetInstance(out SequenceModuleContainer sequenceModuleContainer)
                || sequenceModuleContainer.BattlePauseController == null)
            {
                Debug.LogError($"[{nameof(PauseWindowInitializer)}] {nameof(BattlePauseController)} が見つかりません。", this);
                return false;
            }

            if (!ServiceLocator.TryGetInstance(out StageResultModuleContainer stageResultModuleContainer)
                || stageResultModuleContainer.Controller == null)
            {
                Debug.LogError($"[{nameof(PauseWindowInitializer)}] リスタート用のコントローラーが見つかりません。", this);
                return false;
            }

            if (!ServiceLocator.TryGetInstance(out SceneTransitionUsecase sceneTransitionUsecase))
            {
                Debug.LogError($"[{nameof(PauseWindowInitializer)}] {nameof(SceneTransitionUsecase)} が見つかりません。", this);
                return false;
            }

            if (!ServiceLocator.TryGetInstance(out SelectedBattleStageState selectedBattleStageState))
            {
                Debug.LogError($"[{nameof(PauseWindowInitializer)}] {nameof(SelectedBattleStageState)} が見つかりません。", this);
                return false;
            }

            // ミッション状態は存在すればクリア対象とするため、取得できない場合もnullで許容する。
            ServiceLocator.TryGetInstance(out SelectedMissionState selectedMissionState);

            _battlePauseController = sequenceModuleContainer.BattlePauseController;
            _stageResultController = stageResultModuleContainer.Controller;
            _returnToTitleController = new ReturnToTitleController(
                sceneTransitionUsecase,
                selectedBattleStageState,
                selectedMissionState,
                TITLE_SCENE_NAME);

            _battlePauseController.OnPaused += HandlePaused;
            _battlePauseController.OnResumed += HandleResumed;
            _pauseWindowView.OnRestartRequested += HandleRestartRequested;
            _pauseWindowView.OnReturnToTitleRequested += HandleReturnToTitleRequested;
            _isSubscribed = true;

            return true;
        }

        /// <summary>
        ///     イベント購読を解除します。
        /// </summary>
        public override void Shutdown()
        {
            Unsubscribe();
        }

        /// <summary> 復帰先となるタイトルシーンの名前。 </summary>
        private const string TITLE_SCENE_NAME = "Title";

        [SerializeField, Tooltip("ポーズ中に表示するウィンドウのViewです。")]
        private PauseWindowView _pauseWindowView;

        private BattlePauseController _battlePauseController;
        private StageResultController _stageResultController;
        private ReturnToTitleController _returnToTitleController;
        private bool _isSubscribed;
        private bool _isTransitioning;

        /// <summary>
        ///     破棄時にイベント購読を解除します。
        /// </summary>
        private void OnDestroy()
        {
            Unsubscribe();
        }

        /// <summary>
        ///     ポーズ開始時にウィンドウを表示します。
        /// </summary>
        private void HandlePaused()
        {
            if (_isTransitioning)
            {
                return;
            }

            _pauseWindowView.Show();
        }

        /// <summary>
        ///     ポーズ解除時にウィンドウを非表示にします。
        /// </summary>
        private void HandleResumed()
        {
            _pauseWindowView.Hide();
        }

        /// <summary>
        ///     リスタート要求を受け取ってステージを再読み込みします。
        /// </summary>
        private void HandleRestartRequested()
        {
            // シーン遷移の実処理はUsecase側にあるため、ここでは多重実行だけを防ぐ。
            _ = ExecuteTransitionAsync(() => _stageResultController.RetryAsync(), "ステージのリスタート");
        }

        /// <summary>
        ///     タイトル復帰要求を受け取ってタイトルシーンへ遷移します。
        /// </summary>
        private void HandleReturnToTitleRequested()
        {
            _ = ExecuteTransitionAsync(
                () => _returnToTitleController.ReturnToTitleAsync(
                    gameObject.scene.name,
                    destroyCancellationToken),
                "タイトルへの復帰");
        }

        /// <summary>
        ///     ポーズを解除してからシーン遷移を実行します。
        ///     <para>
        ///         ポーズ中は<see cref="Time.timeScale"/>が0でBGMも停止しているため、
        ///         遷移前に必ずポーズを解除して通常状態へ戻す。
        ///     </para>
        /// </summary>
        /// <param name="transition"> 実行するシーン遷移です。 </param>
        /// <param name="operationName"> 失敗時のログに使用する処理名です。 </param>
        private async Task ExecuteTransitionAsync(Func<Task<bool>> transition, string operationName)
        {
            if (_isTransitioning)
            {
                return;
            }

            _isTransitioning = true;
            _pauseWindowView.SetInteractionEnabled(false);

            try
            {
                _battlePauseController.Resume();

                bool isSucceeded = await transition();

                if (!isSucceeded)
                {
                    Debug.LogError($"[{nameof(PauseWindowInitializer)}] {operationName}に失敗しました。", this);
                    RestorePause();
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                RestorePause();
            }
        }

        /// <summary>
        ///     シーン遷移に失敗した場合に、ポーズ状態とウィンドウ操作を復帰します。
        /// </summary>
        private void RestorePause()
        {
            _isTransitioning = false;

            // Resume()でウィンドウを閉じているため、ポーズし直して再表示する。
            if (!_battlePauseController.Pause())
            {
                _pauseWindowView.Show();
            }

            _pauseWindowView.RestoreInteraction();
        }

        /// <summary>
        ///     イベント購読を解除します。
        /// </summary>
        private void Unsubscribe()
        {
            if (!_isSubscribed)
            {
                return;
            }

            _battlePauseController.OnPaused -= HandlePaused;
            _battlePauseController.OnResumed -= HandleResumed;
            _pauseWindowView.OnRestartRequested -= HandleRestartRequested;
            _pauseWindowView.OnReturnToTitleRequested -= HandleReturnToTitleRequested;
            _isSubscribed = false;
        }
    }
}
