using KillChord.Runtime.Adaptor.InGame.Result;
using KillChord.Runtime.Adaptor.InGame.StageSelect;
using KillChord.Runtime.Adaptor.OutGame.StageSelect;
using KillChord.Runtime.Adaptor.Persistent.Load;
using KillChord.Runtime.Application.InGame.Mission;
using KillChord.Runtime.Application.Persistent.Savedata;
using KillChord.Runtime.Composition.InGame.Bootstrap;
using KillChord.Runtime.Composition.InGame.Mission;
using KillChord.Runtime.Composition.InGame.Player;
using KillChord.Runtime.Composition.InGame.Result;
using KillChord.Runtime.Domain.InGame.Mission;
using KillChord.Runtime.Domain.OutGame.StageSelect;
using KillChord.Runtime.Utility.OutGame.Savedata;
using KillChord.Runtime.View.InGame.Camera;
using KillChord.Runtime.View.InGame.Result;
using KillChord.Runtime.View.InGame.Sequence;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Sequence
{
    /// <summary>
    ///     インゲームシーケンスの構築と進行を担うモジュールです。
    /// </summary>
    public sealed class SequenceInitializationModule : InGameInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(SequenceInitializationModule);

        /// <summary> 実行順です。 </summary>
        public override int Order => 1000;

        /// <summary>
        ///     シーケンス関連ViewとDirectorを解決して公開します。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Build()
        {
            _stageSequenceView = FindFirstObjectByType<StageSequenceView>();
            _stageSequenceMessageView = FindFirstObjectByType<StageSequenceMessageView>();
            _stageStartFadeView = FindFirstObjectByType<StageStartFadeView>();
            _stageStartCameraView = FindFirstObjectByType<StageStartCameraView>();
            _cameraSystemView = FindFirstObjectByType<CameraSystemView>();
            _stageResultView = FindFirstObjectByType<StageResultView>();
            _inGamePlayDirector = FindFirstObjectByType<InGamePlayDirector>();
            _stageSequenceVoiceView = FindFirstObjectByType<StageSequenceVoiceView>();
            _stageStartConstraintView = FindFirstObjectByType<StageStartConstraintView>();

            if (_stageSequenceView == null
                || _stageSequenceMessageView == null
                || _stageStartFadeView == null
                || _stageStartCameraView == null
                || _cameraSystemView == null
                || _stageResultView == null
                || _inGamePlayDirector == null
                || _stageSequenceVoiceView == null
                || _stageStartConstraintView == null)
            {
                Debug.LogError(
                    $"[{nameof(SequenceInitializationModule)}] シーケンス関連参照の取得に失敗しました。",
                    this);
                return false;
            }

            _container = new SequenceModuleContainer();
            ServiceLocator.RegisterInstance(_container);
            _isRegistered = true;
            return true;
        }

        /// <summary>
        ///     リザルトとミッション終了に結合してシーケンスを開始します。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Ready()
        {
            StageResultModuleContainer stageResultContainer =
                ServiceLocator.GetInstance<StageResultModuleContainer>();

            MissionModuleContainer missionContainer =
                ServiceLocator.GetInstance<MissionModuleContainer>();

            PlayerModuleContainer playerContainer =
                ServiceLocator.GetInstance<PlayerModuleContainer>();

            if (stageResultContainer == null
                || missionContainer == null
                || playerContainer == null)
            {
                Debug.LogError(
                    $"[{nameof(SequenceInitializationModule)}] 必要なContainerの取得に失敗しました。",
                    this);
                return false;
            }

            if (stageResultContainer.Presenter == null)
            {
                Debug.LogError(
                    $"[{nameof(SequenceInitializationModule)}] "
                    + $"{nameof(StageResultModuleContainer.Presenter)} が初期化されていません。",
                    this);
                return false;
            }

            if (stageResultContainer.Controller == null)
            {
                Debug.LogError(
                    $"[{nameof(SequenceInitializationModule)}] "
                    + $"{nameof(StageResultModuleContainer.Controller)} が初期化されていません。",
                    this);
                return false;
            }

            if (playerContainer.PlayerView == null)
            {
                Debug.LogError(
                    $"[{nameof(SequenceInitializationModule)}] "
                    + $"{nameof(PlayerModuleContainer.PlayerView)} が初期化されていません。",
                    this);
                return false;
            }

            _stageSequenceVoiceView.Initialize(
                playerContainer.PlayerView);

            _stageResultController = stageResultContainer.Controller;

            _stageStartCameraView.Initialize(
                _cameraSystemView,
                playerContainer.PlayerView.transform);

            _stageStartConstraintView.AddConstraintSource(
                playerContainer.PlayerView.transform);

            _container.SequenceDirector = new InGameSequenceDirector(
                _stageSequenceView,
                _stageSequenceMessageView,
                _stageStartFadeView,
                _stageStartCameraView,
                _stageResultView,
                _stageStartConstraintView,
                stageResultContainer.Presenter,
                _inGamePlayDirector);

            _missionRuntimeService = missionContainer.MissionRuntimeService;
            if (_missionRuntimeService == null)
            {
                Debug.LogError(
                    $"[{nameof(SequenceInitializationModule)}] "
                    + $"{nameof(MissionRuntimeService)} が初期化されていません。",
                    this);
                return false;
            }

            if (!ServiceLocator.TryGetInstance(out _selectedBattleStageState)
                || !ServiceLocator.TryGetInstance(out SavedataSystem savedataSystem))
            {
                Debug.LogError(
                    $"[{nameof(SequenceInitializationModule)}] "
                    + "ステージ選択状態またはセーブシステムを取得できませんでした。",
                    this);
                return false;
            }

            _stageProgressSaveDataService =
                new StageProgressSaveDataService(savedataSystem);

            ServiceLocator.TryGetInstance(out _pendingNodeTransitionState);

            _missionRuntimeService.OnMissionFinished += HandleMissionFinished;
            _inGamePlayDirector.StopGameplay();

            StartAfterLoadingCompleted();
            return true;
        }

        /// <summary>
        ///     登録済みサービスとイベント購読を解除します。
        /// </summary>
        public override void Shutdown()
        {
            UnsubscribeLoadingCompleted();

            _container?.SequenceDirector?.Cancel();

            if (_missionRuntimeService != null)
            {
                _missionRuntimeService.OnMissionFinished -= HandleMissionFinished;
                _missionRuntimeService = null;
            }

            if (_isRegistered)
            {
                ServiceLocator.UnregisterInstance<SequenceModuleContainer>();
                _isRegistered = false;
            }

            _container = null;
            _selectedBattleStageState = null;
            _stageProgressSaveDataService = null;
            _pendingNodeTransitionState = null;
            _loadingScreenController = null;
            _stageResultController = null;
            _hasStarted = false;
            _isEnding = false;
        }

        /// <summary>
        ///     ロード画面の終了後にステージ開始シーケンスを開始します。
        /// </summary>
        private void StartAfterLoadingCompleted()
        {
            if (!ServiceLocator.TryGetInstance(out _loadingScreenController)
                || !_loadingScreenController.IsLoading)
            {
                StartStageSequence();
                return;
            }

            _loadingScreenController.LoadingCompleted += HandleLoadingCompleted;
            _isWaitingForLoadingCompleted = true;
        }

        /// <summary>
        ///     ロード処理の終了通知を受け取ります。
        /// </summary>
        /// <param name="isSuccess"> ロード処理に成功した場合はtrue。 </param>
        private async void HandleLoadingCompleted(bool isSuccess)
        {
            UnsubscribeLoadingCompleted();

            if (isSuccess)
            {
                StartStageSequence();
                return;
            }

            await ReturnToOutGameAfterLoadingFailureAsync();
        }

        /// <summary>
        ///     ロード失敗時に開始演出を中止してOutGameへ戻ります。
        /// </summary>
        private async Task ReturnToOutGameAfterLoadingFailureAsync()
        {
            if (_isEnding)
            {
                return;
            }

            _isEnding = true;

            _container?.SequenceDirector?.Cancel();
            _inGamePlayDirector.StopGameplay();

            // 不完全なステージ画面を表示しないように、
            // OutGameへの遷移が完了するまで黒画面を維持する。
            _stageStartFadeView.ShowBlackImmediate();

            Debug.LogError(
                $"[{nameof(SequenceInitializationModule)}] "
                + "ステージロードに失敗したため、OutGameへ戻ります。",
                this);

            try
            {
                bool success =
                    await _stageResultController.CompleteAsync();

                if (success)
                {
                    return;
                }

                // シーン遷移にも失敗した場合は、
                // 黒画面と入力遮断が残らないように解除する。
                _stageStartFadeView.HideImmediate();

                Debug.LogError(
                    $"[{nameof(SequenceInitializationModule)}] "
                    + "ロード失敗後のOutGame遷移にも失敗しました。",
                    this);
            }
            catch (Exception exception)
            {
                _stageStartFadeView.HideImmediate();
                Debug.LogException(exception, this);
            }
        }

        /// <summary>
        ///     ロード終了イベントの購読を解除します。
        /// </summary>
        private void UnsubscribeLoadingCompleted()
        {
            if (!_isWaitingForLoadingCompleted
                || _loadingScreenController == null)
            {
                return;
            }

            _loadingScreenController.LoadingCompleted -= HandleLoadingCompleted;
            _isWaitingForLoadingCompleted = false;
        }

        /// <summary>
        ///     ステージ開始シーケンスを開始します。
        /// </summary>
        private void StartStageSequence()
        {
            if (_hasStarted
                || _isEnding
                || _container?.SequenceDirector == null)
            {
                return;
            }

            _hasStarted = true;
            _container.SequenceDirector.Start();
        }

        /// <summary>
        ///     ミッション終了に応じてシーケンスを切り替えます。
        /// </summary>
        /// <param name="reason"> 終了理由です。 </param>
        private async void HandleMissionFinished(MissionEndReason reason)
        {
            if (_isEnding || _container?.SequenceDirector == null)
            {
                return;
            }

            _isEnding = true;

            try
            {
                switch (reason)
                {
                    case MissionEndReason.Clear:
                        MissionEvaluationResult evaluationResult =
                            _missionRuntimeService.BuildEvaluationResult();

                        await SaveClearResultAsync(evaluationResult);

                        await _container.SequenceDirector.ClearAsync(
                            evaluationResult,
                            destroyCancellationToken);
                        break;

                    case MissionEndReason.Fail:
                        await _container.SequenceDirector.GameOverAsync(
                            destroyCancellationToken);
                        break;
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        /// <summary>
        ///     確定済み評価結果をステージ進行へ保存します。
        /// </summary>
        /// <param name="evaluationResult"> 保存する評価結果です。 </param>
        private async Task SaveClearResultAsync(MissionEvaluationResult evaluationResult)
        {
            try
            {
                BattleStageDefinition stageDefinition =
                    _selectedBattleStageState.CurrentStageDefinition;

                await _stageProgressSaveDataService.SaveClearAsync(
                    stageDefinition.StageId,
                    evaluationResult,
                    stageDefinition.IsTutorial);

                _pendingNodeTransitionState?.MarkCompleted(
                    stageDefinition.StageId);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }

        private SequenceModuleContainer _container;
        private MissionRuntimeService _missionRuntimeService;
        private StageSequenceView _stageSequenceView;
        private StageSequenceMessageView _stageSequenceMessageView;
        private StageStartFadeView _stageStartFadeView;
        private StageStartCameraView _stageStartCameraView;
        private CameraSystemView _cameraSystemView;
        private StageResultView _stageResultView;
        private StageResultController _stageResultController;
        private InGamePlayDirector _inGamePlayDirector;
        private SelectedBattleStageState _selectedBattleStageState;
        private StageProgressSaveDataService _stageProgressSaveDataService;
        private PendingNodeTransitionState _pendingNodeTransitionState;
        private LoadingScreenController _loadingScreenController;
        private StageSequenceVoiceView _stageSequenceVoiceView;
        private StageStartConstraintView _stageStartConstraintView;
        private bool _isWaitingForLoadingCompleted;
        private bool _isRegistered;
        private bool _hasStarted;
        private bool _isEnding;
    }
}