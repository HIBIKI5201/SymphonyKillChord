using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Application;
using KillChord.Runtime.Composition;
using KillChord.Runtime.Domain;
using KillChord.Runtime.InfraStructure;
using KillChord.Runtime.View;
using KillChord.Structure;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Develop.Assets.Scripts.Develop
{
    public sealed class BattleInitialize : MonoBehaviour
    {
        [SerializeField] private PlayerConfig _playerConfig;
        [SerializeField] private PlayerView _player;
        [SerializeField] private EnemySample _enemySample;

        [Space]

        [Header("キャラクターデータ（テスト用）")]
        [SerializeField] private CharacterData _playerData;
        [SerializeField] private CharacterData _enemyData;

        [Header("アタックパイプライン（テスト用）")]
        [SerializeField] private AttackPipelineAsset _normalPipelineAsset;
        [SerializeField] private AttackPipelineAsset _skillAPipelineAsset;
        [SerializeField] private AttackPipelineAsset _skillBPipelineAsset;
        [SerializeField] private AttackPipelineAsset _ultimatePipelineAsset;
        private void Awake()
        {
            if (_player == null)
                Debug.LogError($"{nameof(PlayerView)}がNullです", this);



            CharacterFactory characterFactory = new CharacterFactory();

            CharacterEntity player = characterFactory.Create(_playerData);
            CharacterEntity enemy = characterFactory.Create(_enemyData);

            Dictionary<AttackId, AttackPipeline> pipelines = new Dictionary<AttackId, AttackPipeline>
            {
                { AttackId.Normal, _normalPipelineAsset.Create() },
                { AttackId.SkillA, _skillAPipelineAsset.Create() },
                { AttackId.SkillB, _skillBPipelineAsset.Create() },
                { AttackId.Ultimate, _ultimatePipelineAsset.Create() },
            };

            AttackPipelineResolver attackPipelineResolver = new(pipelines);
            AttackExecutor attackExecutor = new(attackPipelineResolver);



            InitializePlayer(new BattleController(new BattleApplication(player, attackExecutor), new(), null));
            InitializeEnemy(new BattleController(new BattleApplication(enemy, attackExecutor), new(), new(_enemySample, enemy)));
        }
        private void InitializePlayer(BattleController battleController)
        {
            PlayerMoveParameter parameter = _playerConfig.ToDomain();

            PlayerDodgeMovementApplication dodge = new(parameter);
            PlayerMovement move = new(parameter);
            PlayerApplication application = new(move, dodge);


            PlayerController playerMovementController = new(application);
            _player.Init(playerMovementController, battleController);


#if UNITY_EDITOR
            _player.gameObject
                .AddComponent<PlayerMoveParameterDebug>()
                .SetPlayerMoveParameter(parameter);
#endif
        }
        private void InitializeEnemy(BattleController battleController)
        {
            _enemySample.Init(battleController);
        }
    }
}
