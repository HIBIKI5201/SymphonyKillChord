using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Adaptor.InGame.Camera.Target;
using KillChord.Runtime.Adaptor.InGame.Battle;
using KillChord.Runtime.Adaptor.InGame.Enemy;
using KillChord.Runtime.Adaptor.InGame.Mission;
using KillChord.Runtime.Adaptor.InGame.Music;
using KillChord.Runtime.Application;
using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Application.InGame.Enemy;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Application.InGame.Player;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.Enemy;
using KillChord.Runtime.InfraStructure;
using KillChord.Runtime.InfraStructure.InGame.Character;
using KillChord.Runtime.InfraStructure.InGame.Enemy;
using KillChord.Runtime.View;
using KillChord.Runtime.View.InGame;
using KillChord.Runtime.View.InGame.Enemy;
using SymphonyFrameWork.System.ServiceLocate;
using Unity.Behavior;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Enemy
{
    /// <summary>
    ///     敵歩兵移動の依存関係を構築する。
    /// </summary>
    public class EnemyMoveDebugInitializer : MonoBehaviour
    {
        [SerializeField] private CharacterData _enemyData;
        [SerializeField] private EnemyMoveData _moveData;
        [SerializeField] private EnemyMusicData _encounterMusicData;
        [SerializeField] private EnemyMusicData _battleMusicData;

        [SerializeField] private int _attackIndex;

        [SerializeField] private EnemyMoveView _view;
        [SerializeField] private EnemyRaycastDetectView _raycastView;
        [SerializeField] private NearestAttackPositionSearchView _attackPositionSearchView;
        [SerializeField] private EnemyMissionKeyAsset _missionKeyAsset;
        [SerializeField] private EnemyMovementAIFacade _enemyMovementAIFacade;
        [SerializeField] private EnemyBattleAIFacade _enemyBattleAIFacade;
        [SerializeField] private EnemyStateFacade _enemyStateFacade;
        [SerializeField] private EnemySharedFacade _enemySharedFacade;
        [SerializeField] private BehaviorGraphAgent _behaviorGraphAgent;

        [Header("砲兵の場合のみ必要")]
        [SerializeField] private ShellSpawner _shellSpawner;

        private TargetEntityRegistryController _targetEntityRegistryController;
        private TargetManagerController _targetManagerController;
        private LockOnTargetGateway _lockOnTargetGateway;
        private MissionEventController _missionEventController;
        private CharacterEntity _enemyEntity;
        private IEnemyAttackControllerGenerator _attackControllerGenerator;

        public void Initialize(
            Transform target,
            CharacterEntity targetEntity,
            MusicSyncState musicSyncState,
            IMusicSyncService musicSyncService,
            TargetManagerController targetManagerController,
            TargetEntityRegistryController targetEntityRegistryController,
            IEnemyAttackControllerGenerator attackControllerGenerator
            )
        {
            _targetManagerController = targetManagerController;
            _targetEntityRegistryController = targetEntityRegistryController;
            _enemyEntity = CharacterFactory.Create(_enemyData);

            _missionEventController = ServiceLocator.GetInstance<MissionEventController>();
            _attackControllerGenerator = attackControllerGenerator;
            if (_missionEventController != null && _missionKeyAsset != null)
            {
                _enemyEntity.OnDied += HandleEnemyDied;
            }


            // 敵射線判定
            EnemyRaycastDetectController raycastController = new EnemyRaycastDetectController(_raycastView);
            EnemyRaycastDetectService raycastDetectService = new EnemyRaycastDetectService(raycastController);

            // 攻撃位置探索
            NearestAttackPositionSearchController attackPositionSearchController = new NearestAttackPositionSearchController(_attackPositionSearchView);
            NearestAttackPositionSearchService attackPositionSearchService = new NearestAttackPositionSearchService(attackPositionSearchController);

            // Domain生成
            EnemyMoveSpec spec = EnemyFactory.CreateEnemyMoveSpec(_moveData);
            EnemyAttackMusicSpec attackMusicSpec = EnemyFactory.CreateEnemyAttackMusicSpec(_encounterMusicData, _battleMusicData);

            IMusicActionScheduler musicActionScheduler = new MusicSchedulerAdaptor(musicSyncState, musicSyncService);

            // UseCase
            EnemyMoveUsecase useCase = new EnemyMoveUsecase(spec, raycastDetectService, attackPositionSearchService);
            EnemyAttackReservationUsecase attackReservationUsecase = new EnemyAttackReservationUsecase(attackMusicSpec, musicActionScheduler);
            EnemyAttackUsecase attackUsecase = new EnemyAttackUsecase(raycastDetectService);

            AttackDefinition attackDefinition = _enemyEntity.CombatSpec.GetAttackDifinition(_attackIndex);

            EnemyBattleState battleState = new EnemyBattleState(_enemyEntity, targetEntity, attackDefinition);

            // AttackController生成用コンテキスト
            EnemyAttackControllerContext attackControllerContext = new EnemyAttackControllerContext(attackUsecase, battleState, _shellSpawner);

            // Controller
            IEnemyAttackController attackController = _attackControllerGenerator.Generate(attackControllerContext);
            EnemyAIController controller = new EnemyAIController(useCase, attackReservationUsecase, battleState, _enemyStateFacade, attackController);

            _lockOnTargetGateway = new LockOnTargetGateway(transform);

            _targetManagerController.Register(_lockOnTargetGateway);

            _targetEntityRegistryController.RegisterTargetEntity(_lockOnTargetGateway, _enemyEntity);

            // View接続
            _view.Initialize(controller, target);
            _raycastView.Initialize(target, spec.AttackRangeMax.Value);
            _attackPositionSearchView.Initialize();

            // ファサード初期化
            _enemyMovementAIFacade.Initialize(_view);
            _enemyBattleAIFacade.Initialize(controller);
            _enemyStateFacade.Initialize(controller, target, _raycastView, battleState);
            //_enemySharedFacade.Initialize(target);

            // コンポーネント有効化
            _behaviorGraphAgent.enabled = true;
            _attackPositionSearchView.enabled = true;
        }

        /// <summary>
        ///     敵死亡時に実行する処理。
        /// </summary>
        /// <param name="_"></param>
        private void HandleEnemyDied(CharacterEntity _)
        {
            if (_missionKeyAsset == null)
            {
                return;
            }

            _missionEventController.NotifyEnemyKilled(_missionKeyAsset.Id);
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
