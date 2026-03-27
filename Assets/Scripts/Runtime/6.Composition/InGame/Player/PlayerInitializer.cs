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
        private void Awake()
        {
            if (_player == null)
                Debug.LogError($"{nameof(PlayerView)}がNullです", this);

            PlayerMoveParameter parameter = _playerConfig.ToDomain();
            AttackPipelineResolver attackPipelineResolver = new(new Dictionary<AttackId, AttackPipeline>());
            AttackExecutor attackExecutor = new(attackPipelineResolver);

            AttackCommandState attackCommandState = new();
            AttackBattleState attackBattleState = new();


            PlayerDodgeMovementApplication dodge = new(parameter);
            PlayerMovement move = new(parameter);
            PlayerApplication application = new(move, dodge);

            PlayerController playerMovementController = new(application, attackExecutor, attackCommandState, attackBattleState);
            _player.Init(playerMovementController);


#if UNITY_EDITOR
            _player.gameObject
                .AddComponent<PlayerMoveParameterDebug>()
                .SetPlayerMoveParameter(parameter);
#endif
        }
    }
}
