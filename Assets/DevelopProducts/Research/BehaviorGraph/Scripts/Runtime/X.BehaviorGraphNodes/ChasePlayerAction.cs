using DevelopProducts.BehaviorGraph.Runtime.Adaptor;
using DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Enemy;
using DevelopProducts.BehaviorGraph.Runtime.View.InGame;
using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;
namespace DevelopProducts.BehaviorGraph.Runtime.BehaviorGraphNodes
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "ChasePlayer", story: "Agent Chase Target Until CanAttack", category: "Action", id: "33d48e1dcfc1489ea9d2874a99a9712c")]
    public partial class ChasePlayerAction : Action
    {
        [SerializeReference] public BlackboardVariable<EnemyAIFacade> Facade;
        private EnemyAIFacade _facade;

        protected override Status OnStart()
        {
            if (Facade is null)
            {
                throw new ArgumentNullException("Facade", "[Behavior Graph]ファサードがNULLです。");
            }
            _facade = Facade.Value;
            EnemyMoveInstruction instruction = _facade.GetMovementInstruction();
            _facade.ApplyMovement(instruction);
            return instruction.ShouldMove ? Status.Running : Status.Success;
        }

        protected override Status OnUpdate()
        {
            EnemyMoveInstruction instruction = _facade.GetMovementInstruction();
            _facade.ApplyMovement(instruction);
            return instruction.ShouldMove ? Status.Running : Status.Success;
        }

        protected override void OnEnd()
        {
            
        }
    }
}