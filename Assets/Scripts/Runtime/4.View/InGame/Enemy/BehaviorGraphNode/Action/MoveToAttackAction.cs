using KillChord.Runtime.View;
using KillChord.Runtime.View.InGame.Enemy;
using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ChaseTarget", story: "攻撃可能な位置まで移動 [Movement] [State]", category: "Action", id: "8b82e763f6fed498af18c3983a2c822b")]
public partial class MoveToAttackAction : Action
{
    [SerializeReference] public BlackboardVariable<EnemyMovementAIFacade> Movement;
    [SerializeReference] public BlackboardVariable<EnemyStateFacade> State;
    protected override Status OnStart()
    {
        if (Movement?.Value == null || State?.Value == null) return Status.Failure;
        Movement.Value.MoveToAttack();
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        EnemyStateFacade stateFacade = State.Value;
        if(stateFacade.IsTargetInAttackRange && stateFacade.IsSightClearToAim)
        {
            return Status.Success;
        }
        else
        {
            Movement.Value.MoveToAttack();
            return Status.Running;
        }
    }

    protected override void OnEnd()
    {
        
    }
}

