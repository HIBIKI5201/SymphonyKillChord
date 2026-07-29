using KillChord.Runtime.Adaptor.InGame.Enemy;
using KillChord.Runtime.Adaptor.InGame.Mission;
using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.Adaptor.InGame.Target;
using KillChord.Runtime.Adaptor.InGame.UI;
using KillChord.Runtime.Application.InGame.Enemy;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Composition.InGame.Mission;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.Enemy;
using KillChord.Runtime.Domain.InGame.Mission;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.InfraStructure.Addressables;
using KillChord.Runtime.InfraStructure.InGame.Character;
using KillChord.Runtime.InfraStructure.InGame.Enemy;
using KillChord.Runtime.InfraStructure.InGame.Mission;
using KillChord.Runtime.Utility.Identity;
using KillChord.Runtime.View.InGame.Enemy;
using KillChord.Runtime.View.InGame.Sequence;
using KillChord.Runtime.View.InGame.Target;
using KillChord.Runtime.View.InGame.UI;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

namespace KillChord.Runtime.Composition.InGame.Enemy
{
    /// <summary>
    ///     ボスの依存関係を構築する。
    ///     View / Facade / AIController は Boss 専用型を使う。
    ///     複数攻撃パターンを保持し、攻撃ごとに専用 BattleState + Controller を持つ。
    /// </summary>
    public class BossLifeCycle : MonoBehaviour, IGameplayControllable
    {
        /// <summary>
        ///     ボス用 Addressables アセットをロードします。
        /// </summary>
        /// <param name="cancellationToken"> キャンセルトークンです。 </param>
        /// <returns> 成功した場合はtrue。 </returns>
        public async Task<bool> LoadAddressableAssetsAsync(CancellationToken cancellationToken)
        {
            try
            {
                CharacterDefinitionRepository characterRepository =
                    await _characterRepositoryKey.LoadAssetAsync<CharacterDefinitionRepository>(this, cancellationToken);
                characterRepository?.TryGetAsset(new CharacterDefinitionId(_characterId.Id), out _loadedEnemyData);
                _loadedMoveData = await _moveDataKey.LoadAssetAsync<EnemyMoveSpecAsset>(this, cancellationToken);
                EnemyMissionKeyRepository missionKeyRepository =
                    await _missionKeyRepositoryKey.LoadAssetAsync<EnemyMissionKeyRepository>(this, cancellationToken);
                missionKeyRepository?.TryGetAsset(new EnemyMissionKey(_missionKeyId.Id), out _loadedMissionKeyAsset);
                _loadedAttackEntryRepo = await _attackEntryRepoKey.LoadAssetAsync<BossAttackEntryRepo>(this, cancellationToken);
            }
            catch (Exception ex) { Debug.LogException(ex, this); }

            return _loadedEnemyData != null
                && _loadedMoveData != null
                && _loadedMissionKeyAsset != null
                && _loadedAttackEntryRepo != null;
        }

        /// <summary>
        ///     初期化処理。
        /// </summary>
        /// <remarks>
        ///     attackControllerGenerator は通常敵との互換のため受け取るが、
        ///     ボスは攻撃種別ごとに内部で専用 Generator を使うため未使用。
        /// </remarks>
        public void Initialize(
            Transform target,
            CharacterEntity targetEntity,
            MusicSyncState musicSyncState,
            IMusicSyncService musicSyncService,
            TargetSystemController targetingSystem,
            IEnemyAttackControllerGenerator attackControllerGenerator,
            IShellPool shellPool,
            Action<BossLifeCycle> releaseCallback
            )
        {
            if (_view == null)
            {
                Debug.LogError($"{nameof(BossMoveView)}の参照がありません。");
                return;
            }

            if (_healthView == null)
            {
                Debug.LogError($"{nameof(EnemyHealthView)}の参照がありません。");
                return;
            }

            if (_raycastView == null)
            {
                Debug.LogError($"{nameof(EnemyRaycastDetectView)}の参照がありません。");
                return;
            }

            if (_attackPositionSearchView == null)
            {
                Debug.LogError($"{nameof(NearestAttackPositionSearchView)}の参照がありません。");
                return;
            }

            if (_loadedAttackEntryRepo?.AttackEntries == null || _loadedAttackEntryRepo.AttackEntries.Length == 0)
            {
                Debug.LogError("攻撃エントリ(_attackEntryRepo)が設定されていません。");
                return;
            }
            if (_targetTransform == null)
            {
                Debug.LogError("_targetTransformの参照がありません", this);
                return;
            }

            _targetingSystem = targetingSystem;
            _enemyEntity = CharacterFactory.Create(_loadedEnemyData);

            MissionModuleContainer missionModuleContainer = ServiceLocator.GetInstance<MissionModuleContainer>();
            _missionEventController = missionModuleContainer?.MissionEventController;
            _releaseCallback = releaseCallback;

            // 敵射線判定
            EnemyRaycastDetectController raycastController = new EnemyRaycastDetectController(_raycastView);
            EnemyRaycastDetectController tripleShotRaycastController = new EnemyRaycastDetectController(_tripleShotRaycastView);
            EnemyRaycastDetectService raycastDetectService = new EnemyRaycastDetectService(raycastController);
            EnemyRaycastDetectService tripleRaycastDetectService = new EnemyRaycastDetectService(tripleShotRaycastController);

            // 攻撃位置探索
            NearestAttackPositionSearchController attackPositionSearchController = new NearestAttackPositionSearchController(_attackPositionSearchView);
            NearestAttackPositionSearchService attackPositionSearchService = new NearestAttackPositionSearchService(attackPositionSearchController);

            // Domain生成
            EnemyMoveSpec spec = EnemyFactory.CreateEnemyMoveSpec(_loadedMoveData);

            // Adaptor
            IMusicActionScheduler musicActionScheduler = new MusicSchedulerAdaptor(musicSyncState, musicSyncService);

            // UseCase
            EnemyMoveUsecase moveUsecase = new EnemyMoveUsecase(spec, raycastDetectService, attackPositionSearchService);
            EnemyAttackUsecase attackUsecase = new EnemyAttackUsecase(raycastDetectService);
            EnemyTripleShotAttackUsecase tripleShotAttackUsecase = new EnemyTripleShotAttackUsecase(tripleRaycastDetectService);
            BossAttackReservationUsecase reservationUsecase = new BossAttackReservationUsecase(musicActionScheduler);
            _reservationUsecase = reservationUsecase;

            // AI判定用（移動・硬直・範囲）の戦闘状態。先頭攻撃の定義で初期化する。
            AttackDefinition firstDefinition = _enemyEntity.CombatSpec.GetAttackDifinition(_loadedAttackEntryRepo.AttackEntries[0].AttackIndex);
            EnemyBattleState aiBattleState = new EnemyBattleState(_enemyEntity, targetEntity, firstDefinition);
            _aiBattleState = aiBattleState;

            // 攻撃種別ごとの Generator。
            var generators = new Dictionary<BossAttackKind, IEnemyAttackControllerGenerator>
            {
                { BossAttackKind.Infantry,   new EnemyInfantryAttackControllerGenerator() },
                { BossAttackKind.Artillery,  new EnemyArtilleryAttackControllerGenerator() },
                { BossAttackKind.TripleShot, new EnemyTripleShotAttackControllerGenerator() },
            };

            // 攻撃パターンを構築。各パターンは定義固定の専用 BattleState を持つ。
            var patterns = new List<BossAttackPattern>(_loadedAttackEntryRepo.AttackEntries.Length);
            Dictionary<Type, IRaycastDetectView> raycastViews = new();
            foreach (BossAttackEntryAsset entry in _loadedAttackEntryRepo.AttackEntries)
            {
                AttackDefinition definition = _enemyEntity.CombatSpec.GetAttackDifinition(entry.AttackIndex);
                MusicSyncSpec musicSpec = new MusicSyncSpec(
                    entry.MusicData.BarFlag,
                    entry.MusicData.TimeSignature,
                    entry.MusicData.TargetBeat);

                EnemyBattleState patternState = new EnemyBattleState(_enemyEntity, targetEntity, definition);
                EnemyAttackControllerContext ctx = new EnemyAttackControllerContext(attackUsecase, tripleShotAttackUsecase, patternState, _shellSpawner);
                IEnemyAttackController controller = generators[entry.Kind].Generate(ctx);

                // 通常銃撃と3方向攻撃のRaycastViewを保持する
                if (controller is EnemyInfantryAttackController)
                {
                    raycastViews[typeof(EnemyInfantryAttackController)] = _raycastView;
                }
                if (controller is EnemyTripleShotAttackController)
                {
                    raycastViews[typeof(EnemyTripleShotAttackController)] = _tripleShotRaycastView;
                }

                patterns.Add(new BossAttackPattern(definition, musicSpec, controller));
            }

            // Controller
            BossAIController aiController = new BossAIController(_enemyEntity, moveUsecase, reservationUsecase, aiBattleState, _bossStateFacade, raycastViews, patterns);
            _aiController = aiController;

            IHealthHudViewModel viewModel = new HealthHudViewModel(_enemyEntity.CurrentHealth.Value, _enemyEntity.MaxHealth.Value);
            // HP Presenter
            IHealthHudPresenter healthHudPresenter = new EnemyHealthHudPresenter(_enemyEntity, viewModel, _healthView);
            _healthHudPresenter = healthHudPresenter;

            _targetable = new TransformTargetable(_enemyEntity.Id, transform);

            // View接続
            _view.Initialize(aiController, target);
            _healthView.Bind(viewModel);
            _healthView.Initialize(healthHudPresenter);
            _raycastView.Initialize(target, spec.AttackRangeMax.Value);
            _tripleShotRaycastView.Initialize(target, spec.AttackRangeMax.Value);
            _attackPositionSearchView.Initialize();
            if (_shellSpawner != null && shellPool != null)
            {
                _shellSpawner.Initialize(shellPool);
            }

            // ファサード初期化（Boss専用）
            _bossMovementAIFacade.Initialize(_view);
            _bossBattleAIFacade.Initialize(aiController);
            _bossStateFacade.Initialize(aiController, target, _raycastView, aiBattleState);
            _bossSharedFacade.Initialize(target);
        }

        /// <summary>
        ///     有効化処理。
        /// </summary>
        public void Activate(Vector3 position, System.Action spawnerCallback)
        {
            _spawnerCallback = spawnerCallback;
            _enemyEntity.Reset();
            _aiBattleState.Reset();
            _aiController.Activate();
            _healthHudPresenter.Activate();

            if (_missionEventController != null && _loadedMissionKeyAsset != null)
            {
                _enemyEntity.OnDied += HandleEnemyDied;
            }
            _targetingSystem?.RegisterTarget(_targetable, _enemyEntity);

            // コンポーネント有効化
            _view.Activate();
            _attackPositionSearchView.enabled = true;
            _navMeshAgent.enabled = true;
            _navMeshAgent.Warp(position);
            _behaviorGraphAgent.enabled = true;
            _behaviorGraphAgent.Restart();
            gameObject.SetActive(true);
        }

        /// <summary>
        ///     無効化処理。
        /// </summary>
        public void Deactivate()
        {
            // コンポーネント無効化
            _behaviorGraphAgent.enabled = false;
            _navMeshAgent.enabled = false;
            _attackPositionSearchView.enabled = false;
            _view.Deactivate();

            if (_missionEventController != null && _loadedMissionKeyAsset != null)
            {
                _enemyEntity.OnDied -= HandleEnemyDied;
            }
            _targetingSystem?.UnregisterTarget(_targetable);

            _reservationUsecase.Deactivate();
            _aiController.Deactivate();
            _healthHudPresenter.Deactivate();

            _spawnerCallback?.Invoke();
            _spawnerCallback = null;
            gameObject.SetActive(false);
            _releaseCallback?.Invoke(this);
        }

        /// <summary>
        ///    ゲームプレイ開始処理。
        /// </summary>
        public void StartGameplay()
        {
            if (_behaviorGraphAgent != null)
            {
                _behaviorGraphAgent.enabled = true;
                _behaviorGraphAgent.Restart();
            }

            if (_navMeshAgent != null && _navMeshAgent.enabled)
            {
                _navMeshAgent.isStopped = false;
            }

            _bossBattleAIFacade?.StartGameplay();
            _view?.StartGameplay();
            Activate(transform.position, null);
        }

        /// <summary>
        ///    ゲームプレイ停止処理。
        /// </summary>
        public void StopGameplay()
        {
            _reservationUsecase?.Deactivate();
            _aiController?.CancelAttack();

            if (_behaviorGraphAgent != null)
            {
                _behaviorGraphAgent.enabled = false;
            }

            if (_navMeshAgent != null && _navMeshAgent.enabled && _navMeshAgent.isOnNavMesh)
            {
                _navMeshAgent.isStopped = true;
                _navMeshAgent.ResetPath();
                _navMeshAgent.velocity = Vector3.zero;
                _navMeshAgent.updateRotation = false;
            }

            _bossBattleAIFacade?.StopGameplay();
            _view?.StopGameplay();
            Deactivate();
        }

        private System.Action _spawnerCallback;
        private Action<BossLifeCycle> _releaseCallback;

        [SerializeField, SourceDataAddress, Tooltip("キャラクター定義リポジトリの Addressables キーです。")] private string _characterRepositoryKey;
        [SerializeField, SourceDataCollection("Character"), Tooltip("このボスが対応するキャラクター定義のIDです。")] private DataID _characterId;
        [SerializeField, SourceDataAddress, Tooltip("ボス移動仕様の Addressables キーです。")] private string _moveDataKey;

        [SerializeField] private BossMoveView _view;
        [SerializeField] private EnemyHealthView _healthView;
        [SerializeField] private EnemyRaycastDetectView _raycastView;
        [SerializeField] private TripleShotRaycastDetectView _tripleShotRaycastView;
        [SerializeField] private NearestAttackPositionSearchView _attackPositionSearchView;
        [SerializeField, SourceDataAddress, Tooltip("ボスミッションキーリポジトリの Addressables キーです。")] private string _missionKeyRepositoryKey;
        [SerializeField, SourceDataCollection("EnemyMissionKey"), Tooltip("このボスが対応する敵ミッションキーのIDです。")] private DataID _missionKeyId;
        [SerializeField, SourceDataAddress, Tooltip("ボス攻撃定義群の Addressables キーです。")] private string _attackEntryRepoKey;
        [SerializeField] private BossMovementAIFacade _bossMovementAIFacade;
        [SerializeField] private BossBattleAIFacade _bossBattleAIFacade;
        [SerializeField] private BossStateFacade _bossStateFacade;
        [SerializeField] private BossSharedFacade _bossSharedFacade;
        [SerializeField] private BehaviorGraphAgent _behaviorGraphAgent;
        [SerializeField] private NavMeshAgent _navMeshAgent;

        [SerializeField, Tooltip("敵ロックオン時の中心となるTransform")]
        private Transform _targetTransform;

        [Header("砲撃攻撃を含む場合に必要")]
        [SerializeField] private ShellSpawner _shellSpawner;

        private TargetSystemController _targetingSystem;
        private TransformTargetable _targetable;
        private MissionEventController _missionEventController;
        private CharacterEntity _enemyEntity;
        private BossAIController _aiController;
        private BossAttackReservationUsecase _reservationUsecase;
        private IHealthHudPresenter _healthHudPresenter;
        private EnemyBattleState _aiBattleState;
        private CharacterDefinitionAsset _loadedEnemyData;
        private EnemyMoveSpecAsset _loadedMoveData;
        private EnemyMissionKeyAsset _loadedMissionKeyAsset;
        private BossAttackEntryRepo _loadedAttackEntryRepo;


        /// <summary>
        ///     ボス死亡時に実行する処理。
        /// </summary>
        private void HandleEnemyDied(CharacterEntity _)
        {
            if (_missionEventController != null && _loadedMissionKeyAsset != null)
            {
                _missionEventController.NotifyEnemyKilled(_loadedMissionKeyAsset.Id);
            }
            Deactivate();
        }

        /// <summary>
        ///     ロード済みアセットを解放します。
        /// </summary>
        private void OnDestroy()
        {
            if (_enemyEntity != null)
            {
                _enemyEntity.OnDied -= HandleEnemyDied;
            }

            _targetingSystem?.UnregisterTarget(_targetable);
            _targetable?.Dispose();
            _characterRepositoryKey.ReleaseLoadedAsset(this);
            _moveDataKey.ReleaseLoadedAsset(this);
            _missionKeyRepositoryKey.ReleaseLoadedAsset(this);
            _attackEntryRepoKey.ReleaseLoadedAsset(this);
            _loadedEnemyData = null;
            _loadedMoveData = null;
            _loadedMissionKeyAsset = null;
            _loadedAttackEntryRepo = null;
        }
    }
}

