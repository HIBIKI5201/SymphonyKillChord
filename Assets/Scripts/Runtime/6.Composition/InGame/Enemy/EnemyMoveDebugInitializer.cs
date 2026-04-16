using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Adaptor.InGame;
using KillChord.Runtime.Adaptor.InGame.Battle;
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
using KillChord.Runtime.View.InGame;
using UnityEngine;

namespace KillChord.Runtime.Composition.InGame.Enemy
{
    /// <summary>
    ///     敵移動の依存関係を構築する。
    /// </summary>
    public class EnemyMoveDebugInitializer : MonoBehaviour
    {
        [SerializeField] private CharacterData _enemyData;
        [SerializeField] private EnemyMoveData _moveData;
        [SerializeField] private EnemyMusicData _encounterMusicData;
        [SerializeField] private EnemyMusicData _battleMusicData;

        [SerializeField] private int _attackIndex;

        [SerializeField] private EnemyMoveView _view;

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

            EnemyBattleState battleState = new EnemyBattleState(enemyEntity, targetEntity, attackDefinition);

            // Controller
            EnemyAIController controller = new EnemyAIController(useCase, attackReservationUsecase, attackUsecase, battleState);

            _lockOnTargetGateway = new LockOnTargetGateway(transform);

            _targetManagerController.Register(_lockOnTargetGateway);

            _targetEntityRegistryController.RegisterTargetEntity(_lockOnTargetGateway, enemyEntity);

            // View接続
            _view.Initialize(controller, target);
        }
        private void OnDestroy()
        {
            _targetManagerController?.Unregister(_lockOnTargetGateway);
            _targetEntityRegistryController?.UnregisterTargetEntity(_lockOnTargetGateway);
            _lockOnTargetGateway?.Dispose();
        }
    }
}
