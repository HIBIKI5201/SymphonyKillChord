using KillChord.Runtime.View.InGame.Enemy.AIFacade;
using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy.BehaviorGraphNode.Action
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "MoveToAttack", story: "攻撃可能な位置まで移動する [Movement] [State]", category: "Action", id: "8b82e763f6fed498af18c3983a2c822b")]
    public partial class MoveToAttackAction : Unity.Behavior.Action
    {
        [SerializeReference] public BlackboardVariable<EnemyMovementAIFacade> Movement;
        [SerializeReference] public BlackboardVariable<EnemyStateFacade> State;

        protected override Unity.Behavior.Node.Status OnStart()
        {
            if (Movement?.Value == null || State?.Value == null) return Unity.Behavior.Node.Status.Failure;
            Movement.Value.MoveToAttack();
            return Unity.Behavior.Node.Status.Running;
        }

        protected override Unity.Behavior.Node.Status OnUpdate()
        {
            EnemyStateFacade stateFacade = State.Value;
            if (stateFacade.IsTargetInAttackRange && stateFacade.IsSightClearToAim)
            {
                return Unity.Behavior.Node.Status.Success;
            }

            Movement.Value.MoveToAttack();
            return Unity.Behavior.Node.Status.Running;
        }

        protected override void OnEnd()
        {
        }
    }
}
