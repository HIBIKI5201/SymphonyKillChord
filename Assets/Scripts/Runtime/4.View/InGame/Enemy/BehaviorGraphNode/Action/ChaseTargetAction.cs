using KillChord.Runtime.View;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ChaseTarget", story: "攻撃対象を追跡する [Movement] [State]", category: "Action", id: "8b82e763f6fed498af18c3983a2c822b")]
public partial class ChaseTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<EnemyMovementAIFacade> Movement;
    [SerializeReference] public BlackboardVariable<EnemyStateFacade> State;
    protected override Status OnStart()
    {
        if (Movement?.Value == null) return Status.Failure;
        Movement.Value.ChaseTarget();
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
            Movement.Value.ChaseTarget();
            return Status.Running;
        }
    }

    protected override void OnEnd()
    {
        
    }
}

