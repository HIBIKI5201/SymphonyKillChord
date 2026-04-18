using KillChord.Runtime.View;
using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "IsStunned", story: "Agent [State] Is Stunned", category: "Conditions", id: "842f5b1b693cb1d6b1e5202aa4bcfccc")]
public partial class IsStunnedCondition : Condition
{
    [SerializeReference] public BlackboardVariable<EnemyStateFacade> State;

    public override bool IsTrue()
    {
        // 未実装
        return false;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
