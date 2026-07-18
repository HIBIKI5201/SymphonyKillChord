using KillChord.Runtime.Adaptor.InGame.Target;
using KillChord.Runtime.Adaptor.InGame.Enemy;
using KillChord.Runtime.Adaptor.InGame.Mission;
using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.Adaptor.InGame.UI;
using KillChord.Runtime.Adaptor.InGame.Target;
using KillChord.Runtime.Application.InGame.Enemy;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Composition.InGame.Enemy;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.Enemy;
using KillChord.Runtime.Domain.InGame.Music;
using KillChord.Runtime.InfraStructure.InGame.Character;
using KillChord.Runtime.InfraStructure.InGame.Enemy;
using KillChord.Runtime.InfraStructure.InGame.Mission;
using KillChord.Runtime.View.InGame.Enemy;
using KillChord.Runtime.View.InGame.Sequence;
using KillChord.Runtime.View.InGame.Target;
using KillChord.Runtime.View.InGame.UI;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

namespace DevelopProducts.Boss
{
    /// <summary>
    ///     ボスの依存関係を構築する。
    ///     View / Facade / AIController は Boss 専用型を使う。
    ///     複数攻撃パターンを保持し、攻撃ごとに専用 BattleState + Controller を持つ。
    /// </summary>
    public class BossLifeCycle : MonoBehaviour, IGameplayControllable
    {
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
            TargetSystemController targetSystemController,
            IEnemyAttackControllerGenerator attackControllerGenerator,
            IShellPool shellPool,
            Action<BossLifeCycle> releaseCallback
            )
        {
            if (_view == null)
                Debug.LogError($"{nameof(BossMoveView)}の参照がありません。");
            if (_healthView == null)
                Debug.LogError($"{nameof(EnemyHealthView)}の参照がありません。");
            if (_raycastView == null)
                Debug.LogError($"{nameof(EnemyRaycastDetectView)}の参照がありません。");
            if (_attackPositionSearchView == null)
                Debug.LogError($"{nameof(NearestAttackPositionSearchView)}の参照がありません。");
            if (_attackEntries == null || _attackEntries.Length == 0)
            {
                Debug.LogError("攻撃エントリ(_attackEntries)が設定されていません。");
                return;
            }

            _targetSystemController = targetSystemController;
            _enemyEntity = CharacterFactory.Create(_enemyData);

            _missionEventController = ServiceLocator.GetInstance<MissionEventController>();
            _releaseCallback = releaseCallback;

            // 敵射線判定
            EnemyRaycastDetectController raycastController = new EnemyRaycastDetectController(_raycastView);
            EnemyRaycastDetectService raycastDetectService = new EnemyRaycastDetectService(raycastController);

            // 攻撃位置探索
            NearestAttackPositionSearchController attackPositionSearchController = new NearestAttackPositionSearchController(_attackPositionSearchView);
            NearestAttackPositionSearchService attackPositionSearchService = new NearestAttackPositionSearchService(attackPositionSearchController);

            // Domain生成
            EnemyMoveSpec spec = EnemyFactory.CreateEnemyMoveSpec(_moveData);

            // Adaptor
            IMusicActionScheduler musicActionScheduler = new MusicSchedulerAdaptor(musicSyncState, musicSyncService);

            // UseCase
            EnemyMoveUsecase moveUsecase = new EnemyMoveUsecase(spec, raycastDetectService, attackPositionSearchService);
            EnemyAttackUsecase attackUsecase = new EnemyAttackUsecase(raycastDetectService);
            BossAttackReservationUsecase reservationUsecase = new BossAttackReservationUsecase(musicActionScheduler);
            _reservationUsecase = reservationUsecase;

            // AI判定用（移動・硬直・範囲）の戦闘状態。先頭攻撃の定義で初期化する。
            AttackDefinition firstDefinition = _enemyEntity.CombatSpec.GetAttackDifinition(_attackEntries[0].AttackIndex);
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
            var patterns = new List<BossAttackPattern>(_attackEntries.Length);
            foreach (BossAttackEntry entry in _attackEntries)
            {
                AttackDefinition definition = _enemyEntity.CombatSpec.GetAttackDifinition(entry.AttackIndex);
                MusicSyncSpec timing = new MusicSyncSpec(
                    entry.TimingData.BarFlag,
                    entry.TimingData.TimeSignature,
                    entry.TimingData.TargetBeat);

                EnemyBattleState patternState = new EnemyBattleState(_enemyEntity, targetEntity, definition);
                EnemyAttackControllerContext ctx = new EnemyAttackControllerContext(attackUsecase, null, patternState, _shellSpawner);
                IEnemyAttackController controller = generators[entry.Kind].Generate(ctx);

                patterns.Add(new BossAttackPattern(definition, timing, controller));
            }

            // Controller
            BossAIController aiController = new BossAIController(moveUsecase, reservationUsecase, aiBattleState, _bossStateFacade, patterns);
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
            _aiController.On1BeatBefore += _raycastView.LockWarningDirection;
            _aiController.On2BeatBefore += _raycastView.StartTrackingWarning;
            _aiController.OnAttack += _raycastView.HideWarning;
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

            if (_missionEventController != null && _missionKeyAsset != null)
            {
                _enemyEntity.OnDied += HandleEnemyDied;
            }
            _targetSystemController?.RegisterTarget(_targetable, _enemyEntity);

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

            if (_missionEventController != null && _missionKeyAsset != null)
            {
                _enemyEntity.OnDied -= HandleEnemyDied;
            }
            _targetSystemController?.UnregisterTarget(_targetable);

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
        }

        private System.Action _spawnerCallback;
        private Action<BossLifeCycle> _releaseCallback;

        [SerializeField] private CharacterDefinitionAsset _enemyData;
        [SerializeField] private EnemyMoveSpecAsset _moveData;

        [Header("攻撃パターン（通常1/通常2/特殊1）")]
        [SerializeField] private BossAttackEntry[] _attackEntries;

        [SerializeField] private BossMoveView _view;
        [SerializeField] private EnemyHealthView _healthView;
        [SerializeField] private EnemyRaycastDetectView _raycastView;
        [SerializeField] private NearestAttackPositionSearchView _attackPositionSearchView;
        [SerializeField] private EnemyMissionKeyAsset _missionKeyAsset;
        [SerializeField] private BossMovementAIFacade _bossMovementAIFacade;
        [SerializeField] private BossBattleAIFacade _bossBattleAIFacade;
        [SerializeField] private BossStateFacade _bossStateFacade;
        [SerializeField] private BossSharedFacade _bossSharedFacade;
        [SerializeField] private BehaviorGraphAgent _behaviorGraphAgent;
        [SerializeField] private NavMeshAgent _navMeshAgent;

        [Header("砲撃攻撃を含む場合に必要")]
        [SerializeField] private ShellSpawner _shellSpawner;

        private TargetSystemController _targetSystemController;
        private TransformTargetable _targetable;
        private MissionEventController _missionEventController;
        private CharacterEntity _enemyEntity;
        private BossAIController _aiController;
        private BossAttackReservationUsecase _reservationUsecase;
        private IHealthHudPresenter _healthHudPresenter;
        private EnemyBattleState _aiBattleState;

        /// <summary>
        ///     ボス死亡時に実行する処理。
        /// </summary>
        private void HandleEnemyDied(CharacterEntity _)
        {
            if (_missionEventController != null && _missionKeyAsset != null)
            {
                _missionEventController.NotifyEnemyKilled(_missionKeyAsset.Id);
            }
            Deactivate();

        }

        private void OnDestroy()
        {
            if (_enemyEntity != null)
            {
                _enemyEntity.OnDied -= HandleEnemyDied;
            }

            _targetSystemController?.UnregisterTarget(_targetable);
            _targetable?.Dispose();
        }
    }
}

