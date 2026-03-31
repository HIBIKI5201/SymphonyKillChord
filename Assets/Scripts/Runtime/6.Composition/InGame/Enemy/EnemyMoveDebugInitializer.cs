using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Adaptor.InGame.Battle;
using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Application.InGame.Enemy;
using KillChord.Runtime.Application.InGame.Music;
using KillChord.Runtime.Application.InGame.Player;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.Enemy;
using KillChord.Runtime.InfraStructure;
using KillChord.Runtime.InfraStructure.InGame.Battle;
using KillChord.Runtime.InfraStructure.InGame.Character;
using KillChord.Runtime.InfraStructure.InGame.Enemy;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy
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
        [SerializeField] private AttackPipelineAsset _attackPipelineAsset;

        [SerializeField] private EnemyMoveView _view;

        public void Initialize(
            Transform target,
            IHitTarget targetEntity,
            IMusicSyncViewModel musicSyncViewModel,
            IMusicSyncService musicSyncService
            )
        {
            // Factory
            CharacterFactory characterFactory = new CharacterFactory();
            EnemyFactory factory = new EnemyFactory();

            CharacterEntity enemyEntity = characterFactory.Create(_enemyData);

            // Domain生成
            EnemyMoveSpec spec = factory.CreateEnemyMoveSpec(_moveData);
            EnemyAttackMusicSpec attackMusicSpec = factory.CreateEnemyAttackMusicSpec(_encounterMusicData, _battleMusicData);

            Dictionary<AttackId, AttackPipeline> attackPipelines = new Dictionary<AttackId, AttackPipeline>
            {
                // テスト段階のもので最初の攻撃定義のみパイプラインを作成している。
                {_enemyData.AttackDifinitions[0].AttackId, _attackPipelineAsset.Create() }
            };

            IAttackPipelineResolver attackPipelineResolver = new AttackPipelineResolver(attackPipelines);
            AttackExecutor attackExecutor = new AttackExecutor(attackPipelineResolver);
            IMusicActionScheduler musicActionScheduler = new MusicSchedulerAdaptor(musicSyncViewModel, musicSyncService);

            // UseCase
            EnemyMoveUsecase useCase = new EnemyMoveUsecase(spec);
            EnemyAttackReservationUsecase attackReservationUsecase = new EnemyAttackReservationUsecase(attackMusicSpec, musicActionScheduler);
            EnemyAttackUsecase attackUsecase = new EnemyAttackUsecase(attackExecutor, musicSyncService);

            EnemyBattleState battleState = new EnemyBattleState(enemyEntity, targetEntity, _enemyData.AttackDifinitions[0].AttackId);

            // Controller
            EnemyAIController controller = new EnemyAIController(useCase, attackReservationUsecase, attackUsecase, battleState);

            // View接続
            _view.Initialize(controller, target);
        }
    }
}
