using KillChord.Runtime.Adaptor.InGame.Battle;
using KillChord.Runtime.Adaptor.InGame.Player;
using KillChord.Runtime.Application.InGame.Battle;
using KillChord.Runtime.Application.InGame.Player;
using KillChord.Runtime.Composition.InGame.Enemy;
using KillChord.Runtime.Composition.InGame.Player;
using KillChord.Runtime.Domain.InGame.Battle;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.Player;
using KillChord.Runtime.InfraStructure.InGame.Battle;
using KillChord.Runtime.InfraStructure.InGame.Character;
using KillChord.Runtime.InfraStructure.InGame.Player;
using KillChord.Runtime.Utility;
using KillChord.Runtime.View.InGame.Player;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.Composition
{
    /// <summary>
    ///     プレイヤーに関するクラスの生成と依存関係の解決を行う初期化クラス。
    /// </summary>
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

            CharacterEntity player = CharacterFactory.Create(_playerData);
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
