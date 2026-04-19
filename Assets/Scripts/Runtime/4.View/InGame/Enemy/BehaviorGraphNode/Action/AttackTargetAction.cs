using KillChord.Runtime.View;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "AttackTarget", story: "攻撃対象を攻撃する [Battle] [State]", category: "Action", id: "611c230a6a1f2c1d944d9d2cf1c3a297")]
public partial class AttackTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<EnemyBattleAIFacade> Battle;
    [SerializeReference] public BlackboardVariable<EnemyStateFacade> State;
    protected override Status OnStart()
    {
        if (Battle?.Value == null) return Status.Failure;
        Battle.Value.StartAttack();
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return State.Value.IsAttacking ? Status.Running : Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

