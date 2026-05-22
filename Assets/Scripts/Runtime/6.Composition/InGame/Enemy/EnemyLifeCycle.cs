using KillChord.Runtime.Adaptor.InGame.Camera.Target;
using KillChord.Runtime.Adaptor.InGame.Enemy;
using KillChord.Runtime.Adaptor.InGame.Mission;
using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.Adaptor.InGame.UI;
using KillChord.Runtime.Application.InGame.Enemy;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.Enemy;
using KillChord.Runtime.InfraStructure.InGame.Character;
using KillChord.Runtime.InfraStructure.InGame.Enemy;
using KillChord.Runtime.InfraStructure.InGame.Mission;
using KillChord.Runtime.View.InGame.Enemy;
using KillChord.Runtime.View.InGame.Enemy.AIFacade;
using SymphonyFrameWork.System.ServiceLocate;
using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

namespace KillChord.Runtime.Composition.InGame.Enemy
{
    /// <summary>
    ///     敵の依存関係を構築する。
    /// </summary>
    public class EnemyLifeCycle : MonoBehaviour
    {

        /// <summary>
        ///     初期化処理。
        /// </summary>
        /// <param name="target"></param>
        /// <param name="targetEntity"></param>
        /// <param name="musicSyncState"></param>
        /// <param name="musicSyncService"></param>
        /// <param name="targetManagerController"></param>
        /// <param name="targetEntityRegistryController"></param>
        /// <param name="attackControllerGenerator"></param>
        public void Initialize(
            Transform target,
            CharacterEntity targetEntity,
            MusicSyncState musicSyncState,
            IMusicSyncService musicSyncService,
            TargetManagerController targetManagerController,
            TargetEntityRegistryController targetEntityRegistryController,
            IEnemyAttackControllerGenerator attackControllerGenerator,
            IShellPool shellPool,
            Action<EnemyLifeCycle> releaseCallback
            )
        {
            if (_view == null)
                Debug.LogError($"{nameof(EnemyMoveView)}の参照がありません。");
            if (_healthView == null)
                Debug.LogError($"{nameof(EnemyHealthView)}の参照がありません。");
            if (_raycastView == null)
                Debug.LogError($"{nameof(EnemyRaycastDetectView)}の参照がありません。");
            if (_attackPositionSearchView == null)
                Debug.LogError($"{nameof(NearestAttackPositionSearchView)}の参照がありません。");

            _targetManagerController = targetManagerController;
            _targetEntityRegistryController = targetEntityRegistryController;
            _enemyEntity = CharacterFactory.Create(_enemyData);

            _missionEventController = ServiceLocator.GetInstance<MissionEventController>();
            _attackControllerGenerator = attackControllerGenerator;
            _releaseCallback = releaseCallback;

            // 敵射線判定
            EnemyRaycastDetectController raycastController = new EnemyRaycastDetectController(_raycastView);
            EnemyRaycastDetectService raycastDetectService = new EnemyRaycastDetectService(raycastController);

            // 攻撃位置探索
            NearestAttackPositionSearchController attackPositionSearchController = new NearestAttackPositionSearchController(_attackPositionSearchView);
            NearestAttackPositionSearchService attackPositionSearchService = new NearestAttackPositionSearchService(attackPositionSearchController);

            // Domain生成
            EnemyMoveSpec spec = EnemyFactory.CreateEnemyMoveSpec(_moveData);
            EnemyAttackMusicSpec attackMusicSpec = EnemyFactory.CreateEnemyAttackMusicSpec(_encounterMusicData, _battleMusicData);

            AttackDefinition attackDefinition = _enemyEntity.CombatSpec.GetAttackDifinition(_attackIndex);

            // Adaptor
            IMusicActionScheduler musicActionScheduler = new MusicSchedulerAdaptor(musicSyncState, musicSyncService);
            
            // UseCase
            EnemyMoveUsecase useCase = new EnemyMoveUsecase(spec, raycastDetectService, attackPositionSearchService);
            EnemyAttackReservationUsecase attackReservationUsecase = new EnemyAttackReservationUsecase(attackMusicSpec, musicActionScheduler);
            EnemyAttackUsecase attackUsecase = new EnemyAttackUsecase(raycastDetectService);
            _attackReservationUsecase = attackReservationUsecase;


            EnemyBattleState battleState = new EnemyBattleState(_enemyEntity, targetEntity, attackDefinition);
            _battleState = battleState;

            // AttackController生成用コンテキスト
            EnemyAttackControllerContext attackControllerContext = new EnemyAttackControllerContext(attackUsecase, battleState, _shellSpawner);

            // Controller
            IEnemyAttackController attackController = _attackControllerGenerator.Generate(attackControllerContext);
            EnemyAIController aiController = new EnemyAIController(useCase, attackReservationUsecase, battleState, _enemyStateFacade, attackController);
            _aiController = aiController;

            // HP Presenter
            // TODO 引数2の敵HP HUDは実装待ち
            IHealthHudPresenter healthHudPresenter = new EnemyHealthHudPresenter(_enemyEntity, null);
            _healthHudPresenter = healthHudPresenter;

            _lockOnTargetGateway = new LockOnTargetGateway(transform);

            // View接続
            _view.Initialize(aiController, target);
            _healthView.Initialize(healthHudPresenter);
            _raycastView.Initialize(target, spec.AttackRangeMax.Value);
            _attackPositionSearchView.Initialize();
            if(_shellSpawner != null && shellPool != null)
            {
                _shellSpawner.Initialize(shellPool);
            }

            // ファサード初期化
            _enemyMovementAIFacade.Initialize(_view);
            _enemyBattleAIFacade.Initialize(aiController);
            _enemyStateFacade.Initialize(aiController, target, _raycastView, battleState);
            //_enemySharedFacade.Initialize(target);
        }

        /// <summary>
        ///     有効化処理。
        ///     コンポーネント有効化、インスタンスのリセット、依存や購読の再構築を行う。
        /// </summary>
        public void Activate(Vector3 position, System.Action spawnerCallback)
        {
            _spawnerCallback = spawnerCallback;
            _enemyEntity.Reset();
            _battleState.Reset();
            _aiController.Activate();
            _healthHudPresenter.Activate();

            if (_missionEventController != null && _missionKeyAsset != null)
            {
                _enemyEntity.OnDied += HandleEnemyDied;
            }
            _targetManagerController?.Register(_lockOnTargetGateway);
            _targetEntityRegistryController?.RegisterTargetEntity(_lockOnTargetGateway, _enemyEntity);

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
        ///     コンポーネント無効化、依存解除や購読解除を行う。
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
            _targetManagerController?.Unregister(_lockOnTargetGateway);
            _targetEntityRegistryController?.UnregisterTargetEntity(_lockOnTargetGateway);
            _missionEventController.NotifyEnemyKilled(_missionKeyAsset.Id);
            
            _attackReservationUsecase.Deactivate();
            _aiController.Deactivate();
            _healthHudPresenter.Deactivate();

            _spawnerCallback?.Invoke();
            _spawnerCallback = null;
            gameObject.SetActive(false);
            _releaseCallback?.Invoke(this);

        }
        private System.Action _spawnerCallback;
        private Action<EnemyLifeCycle> _releaseCallback;

        [SerializeField] private CharacterData _enemyData;
        [SerializeField] private EnemyMoveData _moveData;
        [SerializeField] private EnemyMusicData _encounterMusicData;
        [SerializeField] private EnemyMusicData _battleMusicData;

        [SerializeField] private int _attackIndex;

        [SerializeField] private EnemyMoveView _view;
        [SerializeField] private EnemyHealthView _healthView;
        [SerializeField] private EnemyRaycastDetectView _raycastView;
        [SerializeField] private NearestAttackPositionSearchView _attackPositionSearchView;
        [SerializeField] private EnemyMissionKeyAsset _missionKeyAsset;
        [SerializeField] private EnemyMovementAIFacade _enemyMovementAIFacade;
        [SerializeField] private EnemyBattleAIFacade _enemyBattleAIFacade;
        [SerializeField] private EnemyStateFacade _enemyStateFacade;
        [SerializeField] private EnemySharedFacade _enemySharedFacade;
        [SerializeField] private BehaviorGraphAgent _behaviorGraphAgent;
        [SerializeField] private NavMeshAgent _navMeshAgent;

        [Header("砲兵の場合のみ必要")]
        [SerializeField] private ShellSpawner _shellSpawner;

        private TargetEntityRegistryController _targetEntityRegistryController;
        private TargetManagerController _targetManagerController;
        private LockOnTargetGateway _lockOnTargetGateway;
        private MissionEventController _missionEventController;
        private CharacterEntity _enemyEntity;
        private IEnemyAttackControllerGenerator _attackControllerGenerator;
        private EnemyAIController _aiController;
        private EnemyAttackReservationUsecase _attackReservationUsecase;
        private IHealthHudPresenter _healthHudPresenter;
        private EnemyBattleState _battleState;

        /// <summary>
        ///     敵死亡時に実行する処理。
        /// </summary>
        /// <param name="_"></param>
        private void HandleEnemyDied(CharacterEntity _)
        {
            Deactivate();
        }

        private void OnDestroy()
        {
            if (_enemyEntity != null)
            {
                _enemyEntity.OnDied -= HandleEnemyDied;
            }

            _targetManagerController?.Unregister(_lockOnTargetGateway);
            _targetEntityRegistryController?.UnregisterTargetEntity(_lockOnTargetGateway);
            _lockOnTargetGateway?.Dispose();
        }
    }
}
