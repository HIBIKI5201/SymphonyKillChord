using KillChord.Runtime.Adaptor.InGame.Enemy;
using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.Adaptor.InGame.StageSelect;
using KillChord.Runtime.Adaptor.InGame.Target;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Composition.InGame.Bootstrap;
using KillChord.Runtime.Composition.InGame.Music;
using KillChord.Runtime.Composition.InGame.Player;
using KillChord.Runtime.Composition.InGame.Sequence;
using KillChord.Runtime.Composition.InGame.Target;
using KillChord.Runtime.Domain.InGame.Enemy;
using KillChord.Runtime.Domain.InGame.Stage;
using KillChord.Runtime.InfraStructure.Addressables;
using KillChord.Runtime.InfraStructure.InGame.Enemy;
using KillChord.Runtime.Utility.Identity;
using KillChord.Runtime.View.InGame.Enemy;
using KillChord.Runtime.View.InGame.Player;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Enemy
{
    /// <summary>
    ///     敵インスタンスを初期化するクラス。
    ///     Buildフェーズで公開するEnemyModuleContainerはStageEffectInitializerから参照されるため、
    ///     StageEffectInitializerより先に実行される必要があります。
    /// </summary>
    public class EnemyInitializer : InGameInitializationModuleBase
    {
        /// <summary> モジュール名です。 </summary>
        public override string ModuleName => nameof(EnemyInitializer);

        /// <summary> 実行順です。 </summary>
        public override int Order => 700;

        /// <summary>
        ///     Addressables 経由で敵システム用アセットをロードする。
        /// </summary>
        /// <param name="cancellationToken"> キャンセルトークンです。 </param>
        /// <returns> 成功した場合はtrue。 </returns>
        public override async Awaitable<bool> ResourceLoadAsync(CancellationToken cancellationToken)
        {
            if (!ServiceLocator.TryGetInstance(out SelectedBattleStageState selectedBattleStageState)
                || !selectedBattleStageState.HasSelectedBattleStage)
            {
                Debug.LogError(
                    $"[{nameof(EnemyInitializer)}] 選択済みバトルステージを取得できませんでした。",
                    this);
                return false;
            }

            if (string.IsNullOrWhiteSpace(_enemyWaveDefinitionRepositoryKey))
            {
                Debug.LogError(
                    $"[{nameof(EnemyInitializer)}] 敵Wave定義リポジトリのキーが未設定です。",
                    this);
                return false;
            }

            try
            {
                _loadedEnemyWaveDefinitionRepository =
                    await _enemyWaveDefinitionRepositoryKey.LoadAssetAsync<EnemyWaveDefinitionRepository>(
                        this,
                        cancellationToken);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex, this);
                return false;
            }

            if (_loadedEnemyWaveDefinitionRepository == null)
            {
                return false;
            }

            EnemyWaveDefinitionId enemyWaveDefinitionId =
                selectedBattleStageState.CurrentStageDefinition.EnemyWaveDefinitionId;
            if (!_loadedEnemyWaveDefinitionRepository.TryCreateEnemyWaves(
                    enemyWaveDefinitionId,
                    out _loadedEnemyWaves)
                || !_loadedEnemyWaveDefinitionRepository.TryCreateStageEffectCatalog(
                    enemyWaveDefinitionId,
                    out _loadedStageEffectCatalog))
            {
                Debug.LogError(
                    $"[{nameof(EnemyInitializer)}] 敵Wave定義IDに対応するデータが見つかりません。"
                    + $" Id: {enemyWaveDefinitionId.Value}",
                    this);
                return false;
            }

            if (_enemyPools == null || !await _enemyPools.LoadAddressableAssetsAsync(cancellationToken))
            {
                Debug.LogError($"[{nameof(EnemyInitializer)}] 敵プール用アセットのロードに失敗しました。", this);
                return false;
            }

            BossInitializer bossInitializer = GameObject.FindFirstObjectByType<BossInitializer>();
            if (bossInitializer != null && !await bossInitializer.ResourceLoadAsync(cancellationToken))
            {
                Debug.LogError($"[{nameof(EnemyInitializer)}] ボス用アセットのロードに失敗しました。", this);
                return false;
            }

            return true;
        }

        /// <summary>
        ///     敵関連のローカル生成物を構築して公開する。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Build()
        {
            if (!ValidateReferences())
            {
                return false;
            }

            _enemyPools.Initialize();
            _moduleContainer = new EnemyModuleContainer(new EnemyWaveSpawnerState());
            ServiceLocator.RegisterInstance(_moduleContainer);
            _isModuleRegistered = true;
            return true;
        }

        /// <summary>
        ///     他モジュールへ結合して敵システムを初期化する。
        /// </summary>
        /// <returns> 成功した場合はtrue。 </returns>
        public override bool Ready()
        {
            TargetSystemModuleContainer targetSystemContainer = ServiceLocator.GetInstance<TargetSystemModuleContainer>();
            MusicSyncModuleContainer musicSyncContainer = ServiceLocator.GetInstance<MusicSyncModuleContainer>();
            PlayerModuleContainer playerModuleContainer = ServiceLocator.GetInstance<PlayerModuleContainer>();
            if (targetSystemContainer == null
                || musicSyncContainer == null
                || playerModuleContainer == null)
            {
                Debug.LogError($"[{nameof(EnemyInitializer)}] 必要なContainerの取得に失敗しました。", this);
                return false;
            }

            Initialize(targetSystemContainer.TargetSystemController, _enemyPools, _moduleContainer.EnemyWaveSpawnerState);
            _musicSyncService = musicSyncContainer.MusicSyncService;
            _musicSyncState = musicSyncContainer.MusicSyncState;
            _playerInitializer = playerModuleContainer.PlayerInitializer;
            _playerView = playerModuleContainer.PlayerView;

            if (!_initialized)
            {
                return false;
            }

            _enemySpawnPositionSearcher.Initialize(_playerView.transform);
            _enemyInfantrySpawner.Initialize();
            _enemyArtillerySpawner.Initialize();

            _moduleContainer.StageEffectCatalog = _loadedStageEffectCatalog;
            _moduleContainer.EnemyWaveSpawnerController = new EnemyWaveSpawnerController(
                _loadedEnemyWaves,
                _moduleContainer.EnemyWaveSpawnerState,
                _enemyInfantrySpawner,
                _enemyArtillerySpawner,
                _enemyWaveTimerView);
            _enemyWaveTimerView.Initialize(_moduleContainer.EnemyWaveSpawnerController);

            _moduleContainer.BossInitializer = TryInitializeBoss(targetSystemContainer.TargetSystemController, _enemyPools);
            if (_moduleContainer.BossInitializer != null)
            {
                InGamePlayDirector inGamePlayDirector = FindFirstObjectByType<InGamePlayDirector>();
                inGamePlayDirector?.AddGamePlayControllable(_moduleContainer.BossInitializer.LifeCycle);
            }

            return true;
        }

        /// <summary>
        ///     初期化処理。
        /// </summary>
        /// <param name="enemyPools"></param>
        public void Initialize(TargetSystemController targetingSystem, EnemyPools enemyPools, EnemyWaveSpawnerState waveSpawnerState)
        {
            MusicSyncModuleContainer musicSyncContainer = ServiceLocator.GetInstance<MusicSyncModuleContainer>();
            if (musicSyncContainer == null || musicSyncContainer.MusicSyncService == null)
            {
                Debug.LogError("MusicSyncInitializerが見つかりません。", this);
                return;
            }
            _musicSyncService = musicSyncContainer.MusicSyncService;

            if (musicSyncContainer.MusicSyncState == null)
            {
                Debug.LogError("MusicSyncViewが見つかりません。", this);
                return;
            }
            _musicSyncState = musicSyncContainer.MusicSyncState;
            _targetingSystem = targetingSystem;
            PlayerModuleContainer playerModuleContainer = ServiceLocator.GetInstance<PlayerModuleContainer>();
            _playerInitializer = playerModuleContainer?.PlayerInitializer;
            _playerView = playerModuleContainer?.PlayerView;
            if (_playerInitializer == null || _playerView == null)
            {
                Debug.LogError("Playerモジュールの取得に失敗しました。", this);
                return;
            }
            _enemyPools = enemyPools;
            _waveSpawnState = waveSpawnerState;
            _initialized = true;
        }

        /// <summary>
        ///     歩兵の初期化処理。
        /// </summary>
        /// <param name="lifeCycle"></param>
        /// <param name="releaseCallback"></param>
        public void InitializeInfantry(EnemyLifeCycle lifeCycle, Action<EnemyLifeCycle> releaseCallback)
        {
            if (!_initialized)
            {
                Debug.LogError("[EnemyInitializer] 初期化が行われていません。", this);
                return;
            }
            EnemyInfantryAttackControllerGenerator attackControllerGenerator = new EnemyInfantryAttackControllerGenerator();
            lifeCycle.Initialize(_playerView.transform, _playerInitializer.PlayerEntity,
            _musicSyncState, _musicSyncService, _targetingSystem, attackControllerGenerator, null, _waveSpawnState, releaseCallback);
        }

        /// <summary>
        ///     砲兵の初期化処理。
        /// </summary>
        /// <param name="lifeCycle"></param>
        /// <param name="releaseCallback"></param>
        public void InitializeArtillery(EnemyLifeCycle lifeCycle, Action<EnemyLifeCycle> releaseCallback)
        {
            if (!_initialized)
            {
                Debug.LogError("[EnemyInitializer] 初期化が行われていません。", this);
                return;
            }
            EnemyArtilleryAttackControllerGenerator attackControllerGenerator = new EnemyArtilleryAttackControllerGenerator();
            lifeCycle.Initialize(_playerView.transform, _playerInitializer.PlayerEntity,
            _musicSyncState, _musicSyncService, _targetingSystem, attackControllerGenerator, _enemyPools, _waveSpawnState, releaseCallback);
        }

        /// <summary>
        ///     登録済みサービスを解除する。
        /// </summary>
        public override void Shutdown()
        {
            if (_moduleContainer?.EnemyWaveSpawnerController != null)
            {
                _moduleContainer.EnemyWaveSpawnerController.Dispose();
                _moduleContainer.EnemyWaveSpawnerController = null;
            }

            if (_isModuleRegistered)
            {
                ServiceLocator.UnregisterInstance<EnemyModuleContainer>();
                _isModuleRegistered = false;
            }

            _moduleContainer = null;
            _initialized = false;
            _enemyWaveDefinitionRepositoryKey.ReleaseLoadedAsset(this);
            _loadedEnemyWaveDefinitionRepository = null;
            _loadedEnemyWaves = null;
            _loadedStageEffectCatalog = null;
        }

        [SerializeField, SourceDataAddress, Tooltip("敵Wave定義リポジトリのAddressablesキーです。")]
        private string _enemyWaveDefinitionRepositoryKey = "EnemyWaveDefinitionRepository";

        [SerializeField, Tooltip("敵プールです。")]
        private EnemyPools _enemyPools;

        [SerializeField, Tooltip("敵の生成位置探索です。")]
        private EnemySpawnPositionSearcher _enemySpawnPositionSearcher;

        [SerializeField, Tooltip("歩兵スポナーです。")]
        private EnemyInfantrySpawner _enemyInfantrySpawner;

        [SerializeField, Tooltip("砲兵スポナーです。")]
        private EnemyArtillerySpawner _enemyArtillerySpawner;

        [SerializeField, Tooltip("敵ウェーブタイマーViewです。")]
        private EnemyWaveTimerView _enemyWaveTimerView;

        private PlayerInitializer _playerInitializer;
        private PlayerView _playerView;
        private MusicSyncState _musicSyncState;
        private IMusicSyncService _musicSyncService;
        private TargetSystemController _targetingSystem;
        private EnemyWaveSpawnerState _waveSpawnState;
        private bool _initialized = false;
        private bool _isModuleRegistered;
        private EnemyModuleContainer _moduleContainer;
        private EnemyWaveDefinitionRepository _loadedEnemyWaveDefinitionRepository;
        private EnemyWaves _loadedEnemyWaves;
        private IReadOnlyDictionary<int, IStageEffectDefinition> _loadedStageEffectCatalog;

        /// <summary>
        ///     Inspector参照を検証する。
        /// </summary>
        /// <returns> 参照が有効な場合はtrue。 </returns>
        private bool ValidateReferences()
        {
            if (_enemyPools == null
                || _enemySpawnPositionSearcher == null
                || _enemyInfantrySpawner == null
                || _enemyArtillerySpawner == null
                || _enemyWaveTimerView == null)
            {
                Debug.LogError($"[{nameof(EnemyInitializer)}] 敵モジュール参照が不足しています。", this);
                return false;
            }

            return true;
        }

        /// <summary>
        ///     現在シーンからBossInitializerを検索し、初期化を試みる。
        /// </summary>
        /// <param name="targetingSystem"> ターゲット制御です。 </param>
        /// <param name="enemyPools"> 敵プールです。 </param>
        /// <returns> 存在する場合はBossInitializer。 </returns>
        private BossInitializer TryInitializeBoss(TargetSystemController targetingSystem, EnemyPools enemyPools)
        {
            BossInitializer initializer = GameObject.FindFirstObjectByType<BossInitializer>();
            if (initializer == null)
            {
                return null;
            }

            initializer.Initialize(targetingSystem, enemyPools);
            return initializer;
        }
    }
}
