using KillChord.Runtime.Adaptor;
using KillChord.Runtime.Application;
using KillChord.Runtime.Domain;
using KillChord.Runtime.InfraStructure;
using KillChord.Runtime.Utility;
using KillChord.Runtime.View;
using KillChord.Structure;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.Composition
{
    [DefaultExecutionOrder(ExecutionOrderConst.INITIALIZATION)]
    public sealed class PlayerInitializer : MonoBehaviour
    {
        [SerializeField] private PlayerConfig _playerConfig;
        [SerializeField] private PlayerView _player;

        [Space]

        [Header("キャラクターデータ（テスト用）")]
        [SerializeField] private CharacterData _playerData;
        [SerializeField] private CharacterData _enemyData;

        [Header("アタックパイプライン（テスト用）")]
        [SerializeField] private AttackPipelineAsset _normalPipelineAsset;
        [SerializeField] private AttackPipelineAsset _skillAPipelineAsset;
        [SerializeField] private AttackPipelineAsset _skillBPipelineAsset;
        [SerializeField] private AttackPipelineAsset _ultimatePipelineAsset;

        [SerializeField] private EnemyTestSpawner _enemyTestSpawner;

        private void Awake()
        {
            if (_player == null)
                Debug.LogError($"{nameof(PlayerView)}がNullです", this);



            CharacterFactory characterFactory = new CharacterFactory();

            CharacterEntity player = characterFactory.Create(_playerData);
            _enemyTestSpawner.SetTargetEntity(player);

            Dictionary<AttackId, AttackPipeline> pipelines = new Dictionary<AttackId, AttackPipeline>
            {
                { AttackId.Normal, _normalPipelineAsset.Create() },
                { AttackId.SkillA, _skillAPipelineAsset.Create() },
                { AttackId.SkillB, _skillBPipelineAsset.Create() },
                { AttackId.Ultimate, _ultimatePipelineAsset.Create() },
            };




            PlayerMoveParameter parameter = _playerConfig.ToDomain();
            AttackPipelineResolver attackPipelineResolver = new(pipelines);
            AttackExecutor attackExecutor = new(attackPipelineResolver);



            BattleApplication battleApplication = new(player, attackExecutor);
            BattleController battleController = new(battleApplication, new(), null);

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
    }
}
