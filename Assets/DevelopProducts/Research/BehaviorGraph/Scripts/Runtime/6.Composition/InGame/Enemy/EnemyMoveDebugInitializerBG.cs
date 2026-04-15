using DevelopProducts.BehaviorGraph.Runtime.Adaptor;
using DevelopProducts.BehaviorGraph.Runtime.Adaptor.InGame;
using DevelopProducts.BehaviorGraph.Runtime.Adaptor.InGame.Battle;
using DevelopProducts.BehaviorGraph.Runtime.Adaptor.InGame.Enemy;
using DevelopProducts.BehaviorGraph.Runtime.Application.InGame.Battle;
using DevelopProducts.BehaviorGraph.Runtime.Application.InGame.Enemy;
using DevelopProducts.BehaviorGraph.Runtime.Application.InGame.Music;
using DevelopProducts.BehaviorGraph.Runtime.Application.InGame.Player;
using DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Battle;
using DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Character;
using DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Enemy;
using DevelopProducts.BehaviorGraph.Runtime.InfraStructure;
using DevelopProducts.BehaviorGraph.Runtime.InfraStructure.InGame.Character;
using DevelopProducts.BehaviorGraph.Runtime.InfraStructure.InGame.Enemy;
using KillChord.Runtime.InfraStructure;
using KillChord.Runtime.InfraStructure.InGame.Battle;
using KillChord.Runtime.InfraStructure.InGame.Enemy;
using EnemyFactory = DevelopProducts.BehaviorGraph.Runtime.InfraStructure.InGame.Enemy.EnemyFactory;
using UnityEngine;

namespace DevelopProducts.BehaviorGraph.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     敵移動の依存関係を構築する。
    /// </summary>
    public class EnemyMoveDebugInitializerBG : MonoBehaviour
    {
        [SerializeField] private CharacterData _enemyData;
        [SerializeField] private EnemyMoveData _moveData;
        [SerializeField] private EnemyMusicData _encounterMusicData;
        [SerializeField] private EnemyMusicData _battleMusicData;

        [SerializeField] private int _attackIndex;

        [SerializeField] private EnemyMoveViewBG _view;
        [SerializeField] private EnemyAIFacade _aiFacade;

        private TargetEntityRegistryController _targetEntityRegistryController;
        private TargetManagerController _targetManagerController;
        private LockOnTargetGateway _lockOnTargetGateway;
        public void Initialize(
            Transform target,
            CharacterEntity targetEntity,
            IMusicSyncViewModel musicSyncViewModel,
            IMusicSyncService musicSyncService,
            TargetManagerController targetManagerController,
            TargetEntityRegistryController targetEntityRegistryController
            )
        {
            _targetManagerController = targetManagerController;
            _targetEntityRegistryController = targetEntityRegistryController;
            CharacterEntity enemyEntity = CharacterFactory.Create(_enemyData);

            // Domain生成
            EnemyMoveSpec spec = EnemyFactory.CreateEnemyMoveSpec(_moveData);
            EnemyAttackMusicSpec attackMusicSpec = EnemyFactory.CreateEnemyAttackMusicSpec(_encounterMusicData, _battleMusicData);

            IMusicActionScheduler musicActionScheduler = new MusicSchedulerAdaptor(musicSyncViewModel, musicSyncService);

            // UseCase
            EnemyMoveUsecase useCase = new EnemyMoveUsecase(spec);
            EnemyAttackReservationUsecase attackReservationUsecase = new EnemyAttackReservationUsecase(attackMusicSpec, musicActionScheduler);
            EnemyAttackUsecase attackUsecase = new EnemyAttackUsecase(musicSyncService);

            AttackDefinition attackDefinition = enemyEntity.CombatSpec.GetAttackDifinition(_attackIndex);

            EnemyBattleStateBG battleState = new EnemyBattleStateBG(enemyEntity, targetEntity, attackDefinition);

            // Controller
            EnemyAIControllerBG controller = new EnemyAIControllerBG(useCase, attackReservationUsecase, attackUsecase, battleState);

            _lockOnTargetGateway = new LockOnTargetGateway(transform);

            _targetManagerController.Register(_lockOnTargetGateway);

            _targetEntityRegistryController.RegisterTargetEntity(_lockOnTargetGateway, enemyEntity);

            // View接続
            _view.Initialize(controller, target, targetManagerController, battleState);
            _aiFacade.Initialize(_view, controller, battleState, target, transform);
        }
        private void OnDestroy()
        {
            _targetManagerController?.Unregister(_lockOnTargetGateway);
            _targetEntityRegistryController?.UnregisterTargetEntity(_lockOnTargetGateway);
            _lockOnTargetGateway?.Dispose();
        }
    }
}
