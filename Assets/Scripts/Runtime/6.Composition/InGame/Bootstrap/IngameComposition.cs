using KillChord.Runtime.Adaptor.InGame.Enemy;
using KillChord.Runtime.Adaptor.InGame.Mission;
using KillChord.Runtime.Adaptor.InGame.Result;
using KillChord.Runtime.Adaptor.InGame.StageSelect;
using KillChord.Runtime.Adaptor.InGame.Target;
using KillChord.Runtime.Adaptor.Persistent.Load;
using KillChord.Runtime.Application.InGame.Mission;
using KillChord.Runtime.Application.Persistent.Load;
using KillChord.Runtime.Application.Persistent.SceneManagement;
using KillChord.Runtime.Composition.InGame.Camera;
using KillChord.Runtime.Composition.InGame.Enemy;
using KillChord.Runtime.Composition.InGame.Mission;
using KillChord.Runtime.Composition.InGame.Music;
using KillChord.Runtime.Composition.InGame.Player;
using KillChord.Runtime.Composition.InGame.Sequence;
using KillChord.Runtime.Composition.InGame.Target;
using KillChord.Runtime.Composition.InGame.UI;
using KillChord.Runtime.Composition.Persistent.Input;
using KillChord.Runtime.Domain.InGame.Enemy;
using KillChord.Runtime.Domain.InGame.Mission;
using KillChord.Runtime.InfraStructure.InGame.Enemy;
using KillChord.Runtime.Utility.Constant;
using KillChord.Runtime.View;
using KillChord.Runtime.View.InGame.Enemy;
using KillChord.Runtime.View.InGame.Player;
using KillChord.Runtime.View.InGame.Result;
using KillChord.Runtime.View.InGame.Sequence;
using KillChord.Runtime.View.Persistent.Input;
using KillChord.Runtime.View.Persistent.Music;
using SymphonyFrameWork.Attribute;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Bootstrap
{
    /// <summary>
    ///    インゲームシーンの全体的な初期化と構成を担当するクラス。
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class IngameComposition : MonoBehaviour
    {
        [SerializeField] private MusicSyncInitializer _musicSyncInitializer;
        [SerializeField] private CameraSystemInitializer _camerasystemInitializer;
        [SerializeField] private EnemyInfantrySpawner _enemyInfantrySpawner;
        [SerializeField] private EnemyArtillerySpawner _enemyArtillerySpawner;
        [SerializeField] private InGameMissionInitializer _inGameMissionInitializer;
        [SerializeField] private MobileInput _mobileInput;
        [SerializeField] private ACLikeRhythmGuideInitializer _rhythmGuideInitializer;
        [SerializeField, SceneNameSelector] private string _backgroundSceneName;
        [SerializeField] private EnemyPools _enemyPools;
        [SerializeField] private EnemyInitializer _enemyInitializer;
        [SerializeField] private EnemySpawnPositionSearcher _enemySpawnPositionSearcher;
        [SerializeField] private StageSequenceView _stageSequenceView;
        [SerializeField] private StageSequenceMessageView _stageSequenceMessageView;
        [SerializeField] private StageResultView _stageResultView;
        [SerializeField] private InGamePlayDirector _inGamePlayDirector;
        [SerializeField] private EnemyWaveDefinitionAsset _enemyWaveDefinition;
        [SerializeField] private EnemyWaveTimerView _enemyWaveTimerView;

        private PlayerInitializer _playerInitializer;
        private MusicPlayer _musicPlayer;
        private InGameSequenceDirector _inGameSequenceDirector;
        private MissionRuntimeService _missionruntimeService;
        private EnemyWaveSpawnerController _enemyWaveSpawnerController;
        private SelectedBattleStageState _selectedBattleStage;
        private SceneTransitionUsecase _sceneTransition;
        private TargetSystemInitializer _targetSystemInitializer;

        private bool _isEnding = false;

        private async void Start()
        {
            if (!ServiceLocator.TryGetInstance(out _selectedBattleStage))
            {
                Debug.LogError("[IngameComposition] SelectedBattleStageStateが取得できませんでした", this);
                FailActiveLoadingSession();
                return;
            }

            if (!ServiceLocator.TryGetInstance(out ILoadingOperationExecutor operationExecutor))
            {
                Debug.LogError("[IngameComposition] ILoadingOperationExecutorが取得できませんでした", this);
                FailActiveLoadingSession();
                return;
            }

            if (!ServiceLocator.TryGetInstance(out ISceneTransitionService sceneTransitionService))
            {
                Debug.LogError("[IngameComposition] ISceneTransitionServiceが取得できませんでした", this);
                FailActiveLoadingSession();
                return;
            }

            if (!ServiceLocator.TryGetInstance(out _sceneTransition))
            {
                Debug.LogError($"[{nameof(IngameComposition)}] " + $"{nameof(SceneTransitionUsecase)}を取得できませんでした。", this);
                FailActiveLoadingSession();
                return;
            }

            if (!_selectedBattleStage.HasSelectedBattleStage)
            {
                Debug.LogError("[IngameComposition] バトルステージが選択されていません", this);
                FailActiveLoadingSession();
                return;
            }

            var options = LoadingExecutionOptions.ContinueAndComplete(
                LoadingConstants.IN_GAME_SCENE_LOAD_END_PROGRESS,
                1f);

            try
            {
                bool success = await operationExecutor.ExecuteAsync(
                    async totalProgress =>
                    {
                        var stageLoadProgress = new LoadingProgressRange(
                            totalProgress,
                            0f,
                            LoadingConstants.STAGE_LOAD_END_PROGRESS);

                        bool loadSuccess = await sceneTransitionService.LoadAdditiveAsync(
                            _selectedBattleStage.BattleSceneName,
                            stageLoadProgress,
                            destroyCancellationToken
                            );

                        if (!loadSuccess)
                        {
                            Debug.LogError("[IngameComposition] バトルシーンの読み込みに失敗しました", this);
                            return false;
                        }

                        var initializeProgress = new LoadingProgressRange(
                            totalProgress,
                            LoadingConstants.STAGE_LOAD_END_PROGRESS,
                            1f);

                        bool initializeSuccess = await TryInitializeAsync(initializeProgress);

                        if (!initializeSuccess)
                        {
                            Debug.LogError("[IngameComposition] 初期化に失敗しました。");
                            return false;
                        }

                        return true;
                    },
                    options,
                    destroyCancellationToken);

                if (!success)
                {
                    return;
                }

                if (_missionruntimeService == null)
                {
                    Debug.LogError($"[{nameof(IngameComposition)}] " + $"{nameof(MissionRuntimeService)}が初期化されていません。", this);
                    return;
                }

                _missionruntimeService.OnMissionFinished += HandleMissionFinished;
                await _inGameSequenceDirector.StartAsync(destroyCancellationToken);
            }
            catch (OperationCanceledException)
            {

            }
            catch (Exception ex)
            {
                FailActiveLoadingSession();
                Debug.LogException(ex, this);
            }
        }

        private void OnDestroy()
        {
            _enemyWaveSpawnerController?.Dispose();
            _targetSystemInitializer?.Dispose();
            if (_missionruntimeService != null)
            {
                _missionruntimeService.OnMissionFinished -= HandleMissionFinished;
            }
        }

        /// <summary>
        ///     シーン内の必要なコンポーネントやサービスを非同期に初期化する。
        /// </summary>
        /// <returns> 初期化が成功したかどうかを示す値。 </returns>
        private async Awaitable<bool> TryInitializeAsync(IProgress<float> progress)
        {
            progress?.Report(0f);
            _playerInitializer = ServiceLocator.GetInstance<PlayerInitializer>();

            if (_playerInitializer == null)
            {
                Debug.LogError("[IngameComposition] PlayerInitializer の取得に失敗しました。");
                return false;
            }

            var stageSceneI = await ServiceLocator.GetInstanceAsync<IStageSceneInstance>();

            if (stageSceneI == null)
            {
                Debug.LogError(
                    $"[{nameof(IngameComposition)}] " +
                    $"{nameof(IStageSceneInstance)}の取得に失敗しました。",
                    this);

                return false;
            }

            Debug.Log(
                $"stageSceneI {stageSceneI != null}  PlayerT{stageSceneI.PlayerTransform != null} Skill{stageSceneI.SkillInitializer}");

            if (!await WaitMusicPlayerAsync())
            {
                return false;
            }

            UnityEngine.Camera mainCamera = UnityEngine.Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("[IngameComposition] MainCamera が見つかりません。");
                return false;
            }

            if (!ValidateInitialization())
            {
                return false;
            }

            _targetSystemInitializer = new TargetSystemInitializer();
            _targetSystemInitializer.Initialize();

            // 初期化順序の実行
            _musicSyncInitializer.Initialize();

            if (!_inGameMissionInitializer.TryInitialize(out _missionruntimeService))
            {
                Debug.LogError("[IngameComposition] " + "ミッションシステムの初期化に失敗しました。", this);

                return false;
            }

            if (!ServiceLocator.TryGetInstance(out SelectedMissionState selectedMissionState))
            {
                Debug.LogError($"[{nameof(IngameComposition)}] " + $"{nameof(SelectedMissionState)}を取得できませんでした。", this);
                return false;
            }

            StageResultViewModel stageResultViewModel = new();

            StageResultPresenter stageResultPresenter = new(
                    _missionruntimeService,
                    _selectedBattleStage,
                    stageResultViewModel);

            StageResultController stageResultController = new(
                    _sceneTransition,
                    _selectedBattleStage,
                    selectedMissionState);

            _stageResultView.Initialize(stageResultViewModel, stageResultController);

            progress?.Report(0.5f);

            var inputC = ServiceLocator.GetInstance<InputComposition>();
            if (inputC == null)
            {
                Debug.LogError("[IngameComposition] InputComposition の取得に失敗しました。", this);
                return false;
            }

            inputC.GetInputMapController.EnableOnly(InputMapNames.Common);

#if UNITY_ANDROID
            _camerasystemInitializer.Initialize(_targetSystemInitializer.TargetingSystemViewModel);
            _mobileInput.Initialize(inputC.GetInputView);
#else
            _camerasystemInitializer.Initialize(_targetSystemInitializer.TargetingSystemViewModel);
            Cursor.lockState = CursorLockMode.Locked;
#endif

            HUDEnemyHealthInitializer hudEnemyHealthInitializer = FindFirstObjectByType<HUDEnemyHealthInitializer>();
            if (hudEnemyHealthInitializer != null)
            {
                hudEnemyHealthInitializer.Initialize(_targetSystemInitializer.TargetSystemController);
            }

            _playerInitializer.Initialize(inputC);

            PlayerView playerView = FindFirstObjectByType<PlayerView>();

            if (playerView == null)
            {
                Debug.LogError("[IngameComposition] PlayerView が見つかりません。");
                return false;
            }

            _inGamePlayDirector.AddGamePlayControllable(playerView);

            // ステージに事前配置されている敵の情報（現状不要になった）
            //AssignedEnemyManager assignedEnemyManager = FindFirstObjectByType<AssignedEnemyManager>();
            //if (assignedEnemyManager == null)
            //{
            //    Debug.LogWarning("[IngameComposition] 敵の事前配置情報がありません。");
            //}
            //else
            //{
            //    if (assignedEnemyManager.Infantries == null
            //        || assignedEnemyManager.Infantries.Length == 0)
            //    {
            //        Debug.LogWarning("[IngameComposition] 歩兵の事前配置情報がありません。");
            //    }
            //    if (assignedEnemyManager.Artillery == null
            //        || assignedEnemyManager.Artillery.Length == 0)
            //    {
            //        Debug.LogWarning("[IngameComposition] 砲兵の事前配置情報がありません。");
            //    }
            //}

            // 敵生成関連
            _enemyPools.Initialize();

            EnemyWaveSpawnerState enemyWaveSpawnerState = new EnemyWaveSpawnerState();
            _enemyInitializer.Initialize(_targetSystemInitializer.TargetSystemController, _enemyPools, enemyWaveSpawnerState);

            _enemySpawnPositionSearcher.Initialize(_playerInitializer.transform);
            _enemyInfantrySpawner.Initialize();
            _enemyArtillerySpawner.Initialize();

            EnemyWaves enemyWaves = _enemyWaveDefinition.ToDefinition();
            _enemyWaveSpawnerController = new EnemyWaveSpawnerController(enemyWaves, enemyWaveSpawnerState, _enemyInfantrySpawner, _enemyArtillerySpawner, _enemyWaveTimerView);
            _enemyWaveTimerView.Initialize(_enemyWaveSpawnerController);

            // ボス関連
            BossInitializer bossInitializer = TryInitializeBoss(_targetSystemInitializer.TargetSystemController, _enemyPools);
            if (bossInitializer != null)
            {
                _inGamePlayDirector.AddGamePlayControllable(bossInitializer.LifeCycle);
            }

            _rhythmGuideInitializer.Initialize();

            _inGameSequenceDirector = new InGameSequenceDirector(
                _stageSequenceView,
                _stageSequenceMessageView,
                _stageResultView,
                stageResultPresenter,
                _inGamePlayDirector
                );
            _inGamePlayDirector.StopGameplay();

            progress?.Report(1f);
            return true;
        }

        /// <summary>
        ///    MusicPlayer が ServiceLocator から利用可能になるまで待機する。
        /// </summary>
        /// <returns> MusicPlayer が利用可能になったかどうかを示す値。 </returns>
        private async Awaitable<bool> WaitMusicPlayerAsync()
        {
            // 常駐サービスの取得を確実にするため、取得できるまで待機する
            _musicPlayer = ServiceLocator.GetInstance<MusicPlayer>();

            int retryCount = 0;
            while (_musicPlayer == null && retryCount < 20)
            {
                await Awaitable.NextFrameAsync(destroyCancellationToken);
                _musicPlayer = ServiceLocator.GetInstance<MusicPlayer>();
                retryCount++;
            }

            if (_musicPlayer != null)
            {
                return true;
            }

            Debug.LogError("[IngameComposition] MusicPlayer の取得に失敗しました。常駐シーンがロードされているか確認してください。");
            return false;
        }

        /// <summary>
        ///     シーン内の必要なコンポーネントがすべて設定されているかを検証する。
        /// </summary>
        /// <returns> 初期化が有効かどうかを示す値。 </returns>
        private bool ValidateInitialization()
        {
            if (_musicSyncInitializer == null)
            {
                Debug.LogError("[IngameComposition] MusicSyncInitializerの参照が未設定です。", this);
                return false;
            }
            if (_camerasystemInitializer == null)
            {
                Debug.LogError("[IngameComposition] CameraSystemInitializerの参照が未設定です。", this);
                return false;
            }
            if (_enemyPools == null)
            {
                Debug.LogError("[IngameComposition] EnemyPoolsの参照が未設定です。", this);
                return false;
            }
            if (_enemyInitializer == null)
            {
                Debug.LogError("[IngameComposition] EnemyInitializerの参照が未設定です。", this);
                return false;
            }
            if (_enemySpawnPositionSearcher == null)
            {
                Debug.LogError("[IngameComposition] EnemySpawnPositionSearcherの参照が未設定です。", this);
                return false;
            }
            if (_enemyInfantrySpawner == null)
            {
                Debug.LogError("[IngameComposition] EnemyInfantrySpawnerの参照が未設定です。", this);
                return false;
            }
            if (_enemyArtillerySpawner == null)
            {
                Debug.LogError("[IngameComposition] EnemyArtillerySpawnerの参照が未設定です。", this);
                return false;
            }
            if (_rhythmGuideInitializer == null)
            {
                Debug.LogError("[IngameComposition] RhythmGuideInitializerの参照が未設定です。", this);
                return false;
            }
            if (_stageSequenceView == null)
            {
                Debug.LogError("[IngameComposition] StageSequenceViewの参照が未設定です。", this);
                return false;
            }
            if (_inGamePlayDirector == null)
            {
                Debug.LogError("[IngameComposition] InGamePlayDirectorの参照が未設定です。", this);
                return false;
            }
            if (_stageSequenceMessageView == null)
            {
                Debug.LogError("[IngameComposition] StageSequenceMessageViewの参照が未設定です。", this);
                return false;
            }
            if (_inGameMissionInitializer == null)
            {
                Debug.LogError("[IngameComposition] " + "InGameMissionInitializerの参照が未設定です。", this);
                return false;
            }
            if (_stageResultView == null)
            {
                Debug.LogError("[IngameComposition] " + "StageResultViewの参照が未設定です。", this);
                return false;
            }

            return true;
        }

        /// <summary>
        ///     ミッションの終了イベントを処理する。
        ///     ミッションの終了理由に応じて、クリアシーケンスまたはゲームオーバーシーケンスを開始する。
        /// </summary>
        /// <param name="reason">ミッションの終了理由。</param>
        private async void HandleMissionFinished(MissionEndReason reason)
        {
            if (_isEnding)
            {
                return;
            }

            _isEnding = true;

            Debug.Log($"Mission Finished: {reason}");
            switch (reason)
            {
                case MissionEndReason.Clear:
                    await _inGameSequenceDirector.ClearAsync(destroyCancellationToken);
                    break;
                case MissionEndReason.Fail:
                    await _inGameSequenceDirector.GameOverAsync(destroyCancellationToken);
                    break;
            }
        }

        /// <summary>
        ///     実行中のロードセッションを失敗として終了する。
        /// </summary>
        private void FailActiveLoadingSession()
        {
            if (!ServiceLocator.TryGetInstance(out LoadingScreenController loadingScreenController))
            {
                return;
            }

            loadingScreenController.FailActiveSession();
        }

        /// <summary>
        ///     現在シーンからBossInitializerを検索し、初期化を試す。<br/>
        ///     存在する場合、BossInitializerを返却。存在しない場合、nullを返却。
        /// </summary>
        /// <returns></returns>
        private BossInitializer TryInitializeBoss(TargetSystemController targetingSystem, EnemyPools enemyPools)
        {
            // 現在のシーンからBossInitializerを検索する
            BossInitializer initializer = GameObject.FindFirstObjectByType<BossInitializer>();

            // BossInitializerが存在しない場合、処理終了
            if (initializer == null)
            {
                return null;
            }

            initializer.Initialize(targetingSystem, enemyPools);
            return initializer;
        }
    }
}
