using KillChord.Runtime.Application.InGame.Mission;
using KillChord.Runtime.Composition.InGame.Bootstrap;
using KillChord.Runtime.Composition.InGame.Mission;
using KillChord.Runtime.Composition.InGame.Result;
using KillChord.Runtime.Domain.InGame.Mission;
using KillChord.Runtime.View.InGame.Result;
using KillChord.Runtime.View.InGame.Sequence;
using SymphonyFrameWork.System.ServiceLocate;
using System;
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
            _stageResultView = FindFirstObjectByType<StageResultView>();
            _inGamePlayDirector = FindFirstObjectByType<InGamePlayDirector>();

            if (_stageSequenceView == null
                || _stageSequenceMessageView == null
                || _stageResultView == null
                || _inGamePlayDirector == null)
            {
                Debug.LogError($"[{nameof(SequenceInitializationModule)}] シーケンス関連参照の取得に失敗しました。", this);
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
            StageResultModuleContainer stageResultContainer = ServiceLocator.GetInstance<StageResultModuleContainer>();
            MissionModuleContainer missionContainer = ServiceLocator.GetInstance<MissionModuleContainer>();
            if (stageResultContainer == null || missionContainer == null)
            {
                Debug.LogError($"[{nameof(SequenceInitializationModule)}] 必要なContainerの取得に失敗しました。", this);
                return false;
            }

            if (stageResultContainer.Presenter == null)
            {
                Debug.LogError($"[{nameof(SequenceInitializationModule)}] {nameof(StageResultModuleContainer.Presenter)} が初期化されていません。", this);
                return false;
            }

            _container.SequenceDirector = new InGameSequenceDirector(
                _stageSequenceView,
                _stageSequenceMessageView,
                _stageResultView,
                stageResultContainer.Presenter,
                _inGamePlayDirector);

            _missionRuntimeService = missionContainer.MissionRuntimeService;
            if (_missionRuntimeService == null)
            {
                Debug.LogError($"[{nameof(SequenceInitializationModule)}] {nameof(MissionRuntimeService)} が初期化されていません。", this);
                return false;
            }

            _missionRuntimeService.OnMissionFinished += HandleMissionFinished;
            _inGamePlayDirector.StopGameplay();
            StartStageSequenceAsync();
            return true;
        }

        /// <summary>
        ///     登録済みサービスとイベント購読を解除します。
        /// </summary>
        public override void Shutdown()
        {
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
            _isEnding = false;
        }

        /// <summary>
        ///     ステージ開始シーケンスを非同期に開始します。
        /// </summary>
        private async void StartStageSequenceAsync()
        {
            if (_container?.SequenceDirector == null)
            {
                return;
            }

            try
            {
                await _container.SequenceDirector.StartAsync(destroyCancellationToken);
            }
            catch (OperationCanceledException)
            {
            }
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
                        await _container.SequenceDirector.ClearAsync(destroyCancellationToken);
                        break;
                    case MissionEndReason.Fail:
                        await _container.SequenceDirector.GameOverAsync(destroyCancellationToken);
                        break;
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private SequenceModuleContainer _container;
        private MissionRuntimeService _missionRuntimeService;
        private StageSequenceView _stageSequenceView;
        private StageSequenceMessageView _stageSequenceMessageView;
        private StageResultView _stageResultView;
        private InGamePlayDirector _inGamePlayDirector;
        private bool _isRegistered;
        private bool _isEnding;
    }
}
