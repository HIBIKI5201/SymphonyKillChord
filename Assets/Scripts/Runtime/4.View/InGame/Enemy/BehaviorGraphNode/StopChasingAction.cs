using KillChord.Runtime.View;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "StopChasing", story: "追跡を停止する [Movement] [State]", category: "Action", id: "72e36c342c9233772b0a01e15cd5b846")]
public partial class StopChasingAction : Action
{
    [SerializeReference] public BlackboardVariable<EnemyMovementAIFacade> Movement;
    [SerializeReference] public BlackboardVariable<EnemyStateFacade> State;

    protected override Status OnStart()
    {
        Movement.Value.StopChasing();
        return Status.Success;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

